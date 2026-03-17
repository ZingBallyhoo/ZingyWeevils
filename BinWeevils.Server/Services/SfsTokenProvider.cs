using BinWeevils.Common.Database;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BinWeevils.Server.Services
{
    public class SfsTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public SfsTokenProviderOptions()
        {
            Name = nameof(SfsTokenProvider);
            TokenLifespan = TimeSpan.FromMinutes(5);
        }
    }
    
    public class SfsTokenProvider : DataProtectorTokenProvider<WeevilAccount>
    {
        public SfsTokenProvider(IDataProtectionProvider dataProtectionProvider, 
            IOptions<SfsTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<WeevilAccount>> logger) : base(dataProtectionProvider, options, logger)
        {
        }
    }
}