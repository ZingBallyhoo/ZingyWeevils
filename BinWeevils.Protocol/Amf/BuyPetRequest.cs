using PolyType;

namespace BinWeevils.Protocol.Amf
{
    [GenerateShape]
    public partial class BuyPetRequest
    {
        // userID:2,name:3,bowlItemTypeID:4,bedColour:5,bc:6,ac1:7,ac2:8,ec1:9,ec2:10
        // param2,param3,param6,param7,param8,param9,param10,param4,param5
        // userID:2,name:3,bc:6,ac1:7,ac2:8,ec1:9,ec2:10,bowlItemTypeID:4,bedColour:5
        
        public string m_userID;
        public string m_name;
        public uint m_bodyColor;
        public uint m_antenna1Color;
        public uint m_antenna2Color;
        public uint m_eye1Color;
        public uint m_eye2Color;
        public uint m_bowlItemTypeID;
        public uint m_bedColor;
    }
}