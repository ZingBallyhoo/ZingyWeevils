using PolyType;

namespace BinWeevils.Protocol.Form.Business
{
    [GenerateShape]
    public partial class BuyPremisesRequest
    {
        [PropertyShape(Name = "locTypeID")] public uint m_locTypeID { get; set;}
    }
    
    [GenerateShape]
    public partial class BuyPremisesResponse
    {
        [PropertyShape(Name = "res")] public string m_result { get; set; }
        [PropertyShape(Name = "locTypeID")] public uint m_locTypeID { get; set;}
        [PropertyShape(Name = "locID")] public uint m_locID { get; set;}
        
        public const string RESULT_OWNED = "1";
        public const string RESULT_POOR = "2";
        public const string RESULT_SUCCESS = "3";
    }
}