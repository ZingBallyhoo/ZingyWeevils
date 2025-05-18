using BinWeevils.Protocol.Form.Nest;

namespace BinWeevils.Tests.Integration
{
    [Collection("Integration")]
    public class ShopTests
    {
        private readonly IntegrationAppFactory m_factory;

        public ShopTests(IntegrationAppFactory factory)
        {
            m_factory = factory;
        }
        
        [Fact]
        public async Task ItemPaletteDecimal()
        {
            var account = await m_factory.CreateAccount(nameof(ItemPaletteDecimal));
            m_factory.SetAccount(account.UserName!);
            
            var client = m_factory.CreateClient();
            
            var itemTypeID = await m_factory.FindItemByConfigName("wallpaper_tileBathRoom");
            var resp = await client.PostSimpleFormAsync<BuyItemRequest, BuyItemResponse>("api/shop/buyitem", new BuyItemRequest
            {
                m_id = itemTypeID,
                m_shop = "furniture",
                // this was previously parsed as a decimal when seeding palettes
                // causing this request to fail
                m_itemColor = "0x990000"
            });
            Assert.Equal(1, resp.m_result);
        }
    }
}