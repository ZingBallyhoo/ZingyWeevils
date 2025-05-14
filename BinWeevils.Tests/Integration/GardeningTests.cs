using BinWeevils.Protocol.Form.Garden;

namespace BinWeevils.Tests.Integration
{
    [Collection("Integration")]
    public class GardeningTests
    {
        private readonly IntegrationAppFactory m_factory;

        public GardeningTests(IntegrationAppFactory factory)
        {
            m_factory = factory;
        }
        
        [Fact]
        public async Task CreateBasicAccount()
        {
            await m_factory.CreateAccount(nameof(CreateBasicAccount));
        }
        
        [Fact]
        public async Task CantBuyMultipleNonPerishable()
        {
            var account = await m_factory.CreateAccount(nameof(CantBuyMultipleNonPerishable));
            m_factory.SetAccount(account.UserName!);
            
            var client = m_factory.CreateClient();
            var buySeedResponseMessage = await client.PostFormAsync("api/gardenshop/buy-seed", new BuySeedRequest
            {
                m_seedTypeID = await m_factory.FindSeedByConfigName("treeLemon"),
                m_quantity = 5,
            });
            
            Assert.False(buySeedResponseMessage.IsSuccessStatusCode);
        }
        
        [Fact]
        public async Task Plant()
        {
            var account = await m_factory.CreateAccount(nameof(Plant));
            m_factory.SetAccount(account.UserName!);
            
            var client = m_factory.CreateClient();
            var buySeedResponse = await client.PostSimpleFormAsync<BuySeedRequest, BuyGardenItemResponse>("api/gardenshop/buy-seed", new BuySeedRequest
            {
                m_seedTypeID = await m_factory.FindSeedByConfigName("speedySeed"),
                m_quantity = 1,
            });
            
            Assert.Equal(1, buySeedResponse.m_error);
        }
    }
}