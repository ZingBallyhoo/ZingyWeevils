using System.Diagnostics;
using BinWeevils.Database;
using BinWeevils.Protocol;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proto;

namespace BinWeevils.GameServer
{
    public class WeevilSocketServices : IDisposable
    {
        private readonly IServiceProvider m_rootProvider;
        private readonly ILogger<BinWeevilsSocket> m_logger;
        private readonly Activity? m_activity;
        
        public WeevilSocketServices(IServiceProvider rootProvider)
        {
            m_rootProvider = rootProvider;
            m_logger = m_rootProvider.GetRequiredService<ILogger<BinWeevilsSocket>>();
            
            m_activity = GameServerObservability.s_source.StartActivity("socket");
        }
        
        public ActorSystem GetActorSystem()
        {
            return m_rootProvider.GetRequiredService<ActorSystem>();
        }
        
        public LocNameMapper GetLocNameMapper() 
        {
            return m_rootProvider.GetRequiredService<LocNameMapper>();
        }
        
        public ILogger<BinWeevilsSocket> GetLogger()
        {
            return m_logger;
        }
        
        public Activity? GetActivity()
        {
            return m_activity;
        }
        
        public async Task<uint> CreateTempAccount(string name)
        {
            using var loginActivity = GameServerObservability.s_source.StartActivity("Attempt Login");
            loginActivity?.SetTag("userName", name);
            
            await using var scope = m_rootProvider.CreateAsyncScope();
            var identityManager = scope.ServiceProvider.GetRequiredService<UserManager<WeevilAccount>>();
            var account = await identityManager.FindByNameAsync(name);
            if (account != null)
            {
                LoginOkay(name);
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
            LoginOkay(name);
            return account.m_weevilIdx;
        }
        
        private void LoginOkay(string userName)
        {
            m_activity?.SetTag("userName", userName);
        }
        
        public async Task<LoginDto> GetLoginData(uint weevilIdx)
        {
            using var activity = GameServerObservability.StartActivity("Get Login Data");

            await using var scope = m_rootProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<WeevilDBContext>();
            
            return await context.m_weevilDBs
                .Where(x => x.m_idx == weevilIdx)
                .Select(x => new LoginDto
                {
                    m_weevilDef = x.m_weevilDef,
                    m_apparelString = x.m_apparelType == null ? 
                        string.Empty : 
                        $"|{x.m_apparelTypeID}:{context.m_paletteEntries
                            .Single(y => 
                                y.m_paletteID == x.m_apparelType.m_paletteID &&
                                y.m_index == x.m_apparelPaletteEntryIndex)
                            .m_color}"
                }).SingleAsync();
        }

        public async Task<ulong> GetWeevilDef(uint weevilIdx)
        {
            using var activity = GameServerObservability.StartActivity("Get Weevil Def");

            await using var scope = m_rootProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<WeevilDBContext>();
            
            return await context.m_weevilDBs
                .Where(x => x.m_idx == weevilIdx)
                .Select(x => x.m_weevilDef)
                .SingleAsync();
        }
        
        public async IAsyncEnumerable<string> GetBuddies(uint idx)
        {
            using var activity = GameServerObservability.StartActivity("Get Buddies");

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
            using var activity = GameServerObservability.StartActivity("Add Buddy");

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
            using var activity = GameServerObservability.StartActivity("Remove Buddy");

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

        public void Dispose()
        {
            m_activity?.Dispose();
        }

        public async Task<bool> SetApparel(uint weevilIdx, uint apparelID, string color)
        {
            using var activity = GameServerObservability.StartActivity("Set Apparel");
            activity?.SetTag("apparelID", apparelID);
            activity?.SetTag("color", color);
            
            await using var scope = m_rootProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<WeevilDBContext>();
            
            var apparelType = await context.m_apparelTypes
                .Where(x => x.m_id == apparelID)
                .Select(x => new 
                {
                    x.m_paletteID
                })
                .SingleOrDefaultAsync();
            
            if (apparelType == null)
            {
                throw new InvalidDataException($"requested unknown apparel id: {apparelID}");
            }
            
            var paletteEntry = await context.m_paletteEntries
                .Where(x => x.m_paletteID == apparelType.m_paletteID)
                .Where(x => x.m_color == color)
                .Select(x => new
                {
                    x.m_index
                })
                .SingleOrDefaultAsync();
            if (paletteEntry == null)
            {
                throw new InvalidDataException($"requested unknown color \"{color}\" for apparel id {apparelID}");
            }
            
            var rowsUpdated = await context.m_weevilDBs
                .Where(x => x.m_idx == weevilIdx)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_apparelTypeID, apparelID)
                    .SetProperty(x => x.m_apparelPaletteEntryIndex, paletteEntry.m_index)
                );
            
            return rowsUpdated != 0;
        }
        
        public async Task<bool> RemoveApparel(uint weevilIdx)
        {
            using var activity = GameServerObservability.StartActivity("Remove Apparel");
            
            await using var scope = m_rootProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<WeevilDBContext>();
            
            var rowsUpdated = await context.m_weevilDBs
                .Where(x => x.m_idx == weevilIdx)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_apparelTypeID, (uint?)null)
                    .SetProperty(x => x.m_apparelPaletteEntryIndex, 0)
                );
            
            return rowsUpdated != 0;
        }
    }
    
    public class LoginDto
    {
        public ulong m_weevilDef;
        public string m_apparelString;
    }
}