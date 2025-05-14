using ArcticFox.PolyType.FormEncoded;
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
            var response = await client.PostAsync("/api/weevil/data", new FormUrlEncodedContent(new []{new KeyValuePair<string, string>("id", account.UserName!)}));
            response.EnsureSuccessStatusCode();
            
            var weevilData = new FormOptions().Deserialize<WeevilDataResponse>(await response.Content.ReadAsStringAsync());
            Assert.Equal(account.m_weevil.m_idx, weevilData.m_idx);
            Assert.Equal(account.m_weevil.m_weevilDef, weevilData.m_weevilDef);
        }
    }
}