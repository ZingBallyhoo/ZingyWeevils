using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class WeevilDataRequest
    {
        [PropertyShape(Name = "id")] public string m_name { get; set; }
    }
    
    [GenerateShape]
    public partial class WeevilDataResponse
    {
        [PropertyShape(Name = "res")] public int m_result { get; set; }
        [PropertyShape(Name = "idx")] public uint m_idx { get; set; }
        [PropertyShape(Name = "weevilDef")] public ulong m_weevilDef { get; set; }
        [PropertyShape(Name = "level")] public int m_level { get; set; }
        [PropertyShape(Name = "tycoon")] public int m_tycoon { get; set; }
        [PropertyShape(Name = "lastLog")] public string m_lastLog { get; set; }
        [PropertyShape(Name = "dateJoined")] public string m_dateJoined { get; set; }
    }
}