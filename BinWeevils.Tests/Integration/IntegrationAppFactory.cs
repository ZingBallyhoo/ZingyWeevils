using BinWeevils.Common;
using BinWeevils.Common.Database;
using BinWeevils.Protocol;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace BinWeevils.Tests.Integration
{
    public class IntegrationAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IntegrationAuthStorage>();
                
                services
                    .AddAuthentication()
                    .AddScheme<AuthenticationSchemeOptions, IntegrationAuthHandler>("Integration", _ => { });
                services.AddAuthorizationBuilder()
                    .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddAuthenticationSchemes("Integration")
                        .Build());
            });
            base.ConfigureWebHost(builder);
        }

        public async Task<WeevilAccount> CreateAccount(string username)
        {
            await using var scope = Server.Services.CreateAsyncScope();
            
            var identityManager = scope.ServiceProvider.GetRequiredService<UserManager<WeevilAccount>>();
            var initializer = scope.ServiceProvider.GetRequiredService<WeevilInitializer>();
            var weevil = await initializer.Create(new WeevilCreateParams
            {
                m_name = username,
                m_weevilDef = WeevilDef.DEFAULT
            });
            
            var account = new WeevilAccount
            {
                UserName = username,
                m_weevil = weevil
            };
            await identityManager.CreateAsync(account);
            return account;
        }
        
        public void SetAccount(string username)
        {
            var authHandler = Server.Services.GetRequiredService<IntegrationAuthStorage>();
            authHandler.UserName = username;
        }
        
        public async Task<uint> FindSeedByConfigName(string configName)
        {
            await using var scope = Server.Services.CreateAsyncScope();
            var dbc = scope.ServiceProvider.GetRequiredService<WeevilDBContext>();
            
            var seed = await dbc.FindSeedByConfigName(configName);
            if (seed == null)
            {
                throw new NullReferenceException($"unable to find seed: {configName}");
            }
            return seed.Value;
        }
    }
    
    [CollectionDefinition("Integration")]
    public class IntegrationCollection : ICollectionFixture<IntegrationAppFactory>
    {
    }
}