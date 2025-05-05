using BinWeevils.Protocol.Xml;
using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial record HaggleSellRequest
    {
        [PropertyShape(Name = "type")] public EHaggleSaleType m_type;
        [PropertyShape(Name = "item")] public List<uint> m_nestItems;
        [PropertyShape(Name = "gItem")] public List<uint> m_gardenItems;
    }
    
    [GenerateShape]
    public partial record HaggleSellResponse
    {
        [PropertyShape(Name = "err")] public int m_error;
        [PropertyShape(Name = "newMulch")] public int m_newMulch;
    }
}