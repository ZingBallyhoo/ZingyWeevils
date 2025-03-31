using ByteDev.FormUrlEncoded;

namespace BinWeevils.Protocol.Form
{
    public class AdPathsRequest
    {
        [FormUrlEncodedPropertyName("area")] public AdPathsArea m_area { get; set; }
    }
    
    public enum AdPathsArea
    {
        LOADER = 0,
        UNK_1, // todo: where :)
        OUTSIDE_SHOPPING_MALL,
        RUMS_COVE,
        FLEM_MANOR,
        PARTY_BOX,
        DIRT_VALLEY_1,
        RIGGS_PALLADIUM
    }
}