using PolyType;

namespace BinWeevils.Protocol.Form.Pet
{
    [GenerateShape]
    public partial class GetPetFoodStockRequest
    {
        [PropertyShape(Name = "userID")] public string m_userID;
    }
    
    [GenerateShape]
    public partial class GetPetFoodStockResponse
    {
        [PropertyShape(Name = "result")] public uint m_result;
    }
}