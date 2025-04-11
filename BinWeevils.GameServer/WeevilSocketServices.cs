using BinWeevils.Database;
using BinWeevils.Protocol;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BinWeevils.GameServer
{
    public class WeevilSocketServices
    {
        private readonly IServiceProvider m_rootProvider;
        
        public WeevilSocketServices(IServiceProvider rootProvider)
        {
            m_rootProvider = rootProvider;
        }
        
        public async Task<int> CreateTempAccount(string name)
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
                
            var dbWeevil = new WeevilDB
            {
                m_name = name,
                m_weevilDef = ulong.Parse(weevilDef),
                m_food = 100,
                m_fitness = 100,
                m_happiness = 100,
                m_xp = 146138,
                m_lastAcknowledgedLevel = 1,
                m_mulch = 2000,
                m_dosh = 0
            };
            var result = await identityManager.CreateAsync(new WeevilAccount
            {
                UserName = name,
                Email = $"{name}@weevilmail.net",
                m_weevil = dbWeevil
            }, "h");
            if (!result.Succeeded) throw new Exception($"Create account failed: {result}");
            
            account = await identityManager.FindByNameAsync(name);
            if (account == null) throw new NullReferenceException(nameof(account));
            return account.m_weevilIdx;
        }
        
        public async Task<LoginDto> GetLoginData(int weevilIdx)
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
    }
    
    public class LoginDto
    {
        public ulong m_weevilDef;
    }
}