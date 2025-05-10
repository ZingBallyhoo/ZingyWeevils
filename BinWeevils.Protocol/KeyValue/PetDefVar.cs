using PolyType;

namespace BinWeevils.Protocol.KeyValue
{
    [GenerateShape]
    public partial record struct PetDefVar
    {
        [PropertyShape(Name = "id")] public uint m_id;
        [PropertyShape(Name = "name")] public string m_name;
        [PropertyShape(Name = "bc")] public uint m_bodyColor;
        [PropertyShape(Name = "ac1")] public uint m_antenna1Color;
        [PropertyShape(Name = "ac2")] public uint m_antenna2Color;
        [PropertyShape(Name = "ec1")] public uint m_eye1Color;
        [PropertyShape(Name = "ec2")] public uint m_eye2Color;

        public override string ToString()
        {
            return $"id:{m_id},name:{m_name},bc:{m_bodyColor},ac1:{m_antenna1Color},ac2:{m_antenna2Color},ec1:{m_eye1Color},ec2:{m_eye2Color}";
        }
    }
}