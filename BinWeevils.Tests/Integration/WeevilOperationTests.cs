using BinWeevils.Protocol.Form.Weevil;

namespace BinWeevils.Tests.Integration
{
    public class WeevilOperationTests : IClassFixture<IntegrationAppFactory>
    {
        private readonly IntegrationAppFactory m_factory;

        public WeevilOperationTests(IntegrationAppFactory factory)
        {
            m_factory = factory;
        }
        
        [Fact]
        public async Task CreateBasicAccount()
        {
            await m_factory.CreateAccount(nameof(CreateBasicAccount));
        }
        
        [Fact]
        public async Task FetchData()
        {
            var account = await m_factory.CreateAccount(nameof(FetchData));
            m_factory.SetAccount(account.UserName!);
            
            var client = m_factory.CreateClient();
            var response = await client.PostSimpleFormAsync<WeevilDataRequest, WeevilDataResponse>("/api/weevil/data", new WeevilDataRequest
            {
                m_name = account.UserName!
            });
            
            Assert.Equal(account.m_weevil.m_idx, response.m_idx);
            Assert.Equal(account.m_weevil.m_weevilDef, response.m_weevilDef);
        }
    }
}