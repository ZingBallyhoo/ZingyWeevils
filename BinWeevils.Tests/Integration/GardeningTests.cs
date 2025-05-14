using BinWeevils.Protocol.Form.Garden;
using BinWeevils.Protocol.Xml;
using StackXML;

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
        public async Task CanBuyMultiplePerishable()
        {
            var account = await m_factory.CreateAccount(nameof(CanBuyMultiplePerishable));
            m_factory.SetAccount(account.UserName!);
            
            var client = m_factory.CreateClient();
            var buySeedResponseMessage = await client.PostFormAsync("api/gardenshop/buy-seed", new BuySeedRequest
            {
                m_seedTypeID = await m_factory.FindSeedByConfigName("speedySeed"),
                m_quantity = 5,
            });
            
            Assert.True(buySeedResponseMessage.IsSuccessStatusCode);
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
        public async Task GrowPerishable()
        {
            var account = await m_factory.CreateAccount(nameof(GrowPerishable));
            m_factory.SetAccount(account.UserName!);
            
            var client = m_factory.CreateClient();
            
            var storedItemsResp = await client.PostFormAsync("api/garden/get-stored-items", new GetStoredGardenItemsRequest
            {
                m_userID = account.UserName!
            });
            storedItemsResp.EnsureSuccessStatusCode();
            var storedItems = XmlReadBuffer.ReadStatic<StoredGardenItems>(await storedItemsResp.Content.ReadAsStringAsync());
            
            Assert.Empty(storedItems.m_items);
            Assert.Single(storedItems.m_seeds);
            
            await client.PostSimpleFormAsync("api/garden/add-plant", new AddPlantRequest
            {
                m_plantID = storedItems.m_seeds.First().m_databaseID,
                m_x = -40,
                m_z = 288
            });
            
            var earlyHarvestResponse = await client.PostFormAsync("api/garden/harvest-plant", new HarvestPlantRequest
            {
                m_plantID = storedItems.m_seeds.First().m_databaseID
            });
            Assert.False(earlyHarvestResponse.IsSuccessStatusCode);
            
            // todo: remove hardcoded (~)grow time
            m_factory.AdvanceTime(TimeSpan.FromMinutes(3));
            
            // should succeed now
            await client.PostSimpleFormAsync("api/garden/harvest-plant", new HarvestPlantRequest
            {
                m_plantID = storedItems.m_seeds.First().m_databaseID
            });
            
            // shouldn't succeed, this is a perishable plant that was harvested aka removed
            var removeResponse = await client.PostFormAsync("api/garden/remove-plant", new HarvestPlantRequest
            {
                m_plantID = storedItems.m_seeds.First().m_databaseID
            });
            Assert.False(removeResponse.IsSuccessStatusCode);
        }
    }
}