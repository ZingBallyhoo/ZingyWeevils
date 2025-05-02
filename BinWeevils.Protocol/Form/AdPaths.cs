using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class AdPathsRequest
    {
        [PropertyShape(Name = "area")] public AdPathsArea m_area { get; set; }
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
    
    [GenerateShape]
    public partial class LoaderAdPathsResponse
    {
        [PropertyShape(Name = "paths")] public List<string> m_paths { get; set; } = new List<string>();
    }
    
    [GenerateShape]
    public partial class TwoAdPathsResponse
    {
        [PropertyShape(Name = "ad1Path")] public string m_ad1Path { get; set; } = "0";
        [PropertyShape(Name = "ad2Path")] public string m_ad2Path { get; set; } = "0";
        [PropertyShape(Name = "ad1Link")] public string m_ad1Link { get; set; } = "0";
        [PropertyShape(Name = "ad2Link")] public string m_ad2Link { get; set; } = "0";
    }
}