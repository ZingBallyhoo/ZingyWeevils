using PolyType;

namespace BinWeevils.Protocol.Amf
{
    [GenerateShape]
    public partial class BuyPetFoodRequest
    {
        public string m_userID;
        public string m_type;
    }
}