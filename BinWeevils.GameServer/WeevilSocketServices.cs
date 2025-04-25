using BinWeevils.Database;
using BinWeevils.Protocol;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Proto;

namespace BinWeevils.GameServer
{
    public class WeevilSocketServices
    {
        private readonly IServiceProvider m_rootProvider;
        
        public WeevilSocketServices(IServiceProvider rootProvider)
        {
            m_rootProvider = rootProvider;
        }
        
        public async Task<uint> CreateTempAccount(string name)
        {
            await using var scope = m_rootProvider.CreateAsyncScope();
            var identityManager = scope.ServiceProvider.GetRequiredService<UserManager<WeevilAccount>>();
            var account = await identityManager.FindByNameAsync(name);
            if (account != null)
            {
                return account.m_weevilIdx;
            }
            
            string weevilDef;
            if (name.Contains("fairriver"))
            {
                weevilDef = WeevilDef.DEFAULT;
            } else if (name.Contains("zingy"))
            {
                weevilDef = WeevilDef.ZINGY;
            } else if (name.Contains("scribbles"))
            {
                weevilDef = WeevilDef.DEFINITELY_SCRIBBLES;
            } else if (name.Contains("coolcat"))
            {
                weevilDef = "1061064041061709";
            } else
            {
                var def = new WeevilDef(WeevilDef.DEFAULT)
                {
                    m_headType = (WeevilDef.HeadType)Random.Shared.Next((int)WeevilDef.HeadType.Spheroid, (int)WeevilDef.HeadType.Count),
                    m_headColorIdx = (byte)Random.Shared.Next(0, WeevilDef.LEGACY_COLOR_COUNT),
                    m_bodyType = (WeevilDef.BodyType)Random.Shared.Next((int)WeevilDef.BodyType.Spheroid, (int)WeevilDef.BodyType.Count),
                    m_bodyColorIdx = (byte)Random.Shared.Next(0, WeevilDef.LEGACY_COLOR_COUNT),
                    m_eyeType = (WeevilDef.EyeType)Random.Shared.Next((int)WeevilDef.EyeType.MiddleTogether, (int)WeevilDef.EyeType.Count),
                    m_eyeColorIdx = (byte)Random.Shared.Next(0, WeevilDef.LEGACY_EYE_COLOR_COUNT),
                    m_lids = Random.Shared.Next(0, 1) == 1,
                    m_antennaType = (WeevilDef.AntennaType)Random.Shared.Next(0, (int)WeevilDef.AntennaType.SuperOriginal + 1),
                    m_antennaColorIdx = (byte)Random.Shared.Next(0, WeevilDef.LEGACY_COLOR_COUNT),
                    m_legColorIdx = (byte)Random.Shared.Next(0, WeevilDef.LEGACY_COLOR_COUNT),
                    m_legType = WeevilDef.LegType.Normal
                };
                if (!def.Validate())
                {
                    throw new InvalidDataException();
                }
                
                weevilDef = def.AsString();
            }
            
            var context = scope.ServiceProvider.GetRequiredService<WeevilDBContext>();
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            var dbWeevil = await context.CreateWeevil(new WeevilCreateParams
            {
                m_name = name,
                m_weevilDef = ulong.Parse(weevilDef)
            });
            
            var result = await identityManager.CreateAsync(new WeevilAccount
            {
                UserName = name,
                Email = $"{name}@weevilmail.net",
                m_weevil = dbWeevil
            }, "h");
            if (!result.Succeeded) throw new Exception($"Create account failed: {result}");
            
            account = await identityManager.FindByNameAsync(name);
            if (account == null) throw new NullReferenceException(nameof(account));
            
            await transaction.CommitAsync();
            return account.m_weevilIdx;
        }
        
        public async Task<LoginDto> GetLoginData(uint weevilIdx)
        {
            await using var scope = m_rootProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<WeevilDBContext>();
            
            return await context.m_weevilDBs
                .Where(x => x.m_idx == weevilIdx)
                .Select(x => new LoginDto
                {
                    m_weevilDef = x.m_weevilDef,
                }).SingleAsync();;
        }
        
        public ActorSystem GetActorSystem()
        {
            return m_rootProvider.GetRequiredService<ActorSystem>();
        }
        
        public LocNameMapper GetLocNameMapper() 
        {
            return m_rootProvider.GetRequiredService<LocNameMapper>();
        }
        
        public async IAsyncEnumerable<string> GetBuddies(uint idx)
        {
            await using var scope = m_rootProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<WeevilDBContext>();
            
            var results1 = context.m_buddyRecords
                .Where(x => x.m_weevil1ID == idx)
                .Select(x => x.m_weevil2.m_name);
            var results2 = context.m_buddyRecords
                .Where(x => x.m_weevil2ID == idx)
                .Select(x => x.m_weevil1.m_name);
            
            await foreach (var buddyName in results1.Concat(results2).AsAsyncEnumerable())
            {
                yield return buddyName;
            }
        }
        
        public async Task<bool> AddBuddy(string weevil1, string weevil2)
        {
            await using var scope = m_rootProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<WeevilDBContext>();
            
            var weevil1ID = await context.m_weevilDBs.Where(x => x.m_name == weevil1).Select(x => x.m_idx).SingleAsync();
            var weevil2ID = await context.m_weevilDBs.Where(x => x.m_name == weevil2).Select(x => x.m_idx).SingleAsync();
            
            await context.m_buddyRecords.AddAsync(new BuddyRecordDB
            {
                m_weevil1ID = weevil1ID,
                m_weevil2ID = weevil2ID
            });
            try
            {
                await context.SaveChangesAsync();
            } catch (DbUpdateException)
            {
                // lost a race
                return false;
            }
            return true;
        }
        
        public async Task<bool> RemoveBuddy(string weevil1, string weevil2)
        {
            await using var scope = m_rootProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<WeevilDBContext>();
            
            var weevil1ID = await context.m_weevilDBs.Where(x => x.m_name == weevil1).Select(x => x.m_idx).SingleAsync();
            var weevil2ID = await context.m_weevilDBs.Where(x => x.m_name == weevil2).Select(x => x.m_idx).SingleAsync();
            
            var rowsUpdated = await context.m_buddyRecords
                .Where(x => 
                    x.m_weevil1ID == weevil1ID && x.m_weevil2ID == weevil2ID ||
                    x.m_weevil2ID == weevil1ID && x.m_weevil1ID == weevil2ID)
                .ExecuteDeleteAsync();
            return rowsUpdated != 0;
        }
    }
    
    public class LoginDto
    {
        public ulong m_weevilDef;
    }
}