using PolyType;

namespace BinWeevils.Protocol.KeyValue
{
    [GenerateShape]
    public partial struct PetState
    {
        [PropertyShape(Name = "locID")] public int m_locID;
        [PropertyShape(Name = "pose")] public EPetAction m_pose;
        [PropertyShape(Name = "x")] public double m_x;
        [PropertyShape(Name = "y")] public double m_y;
        [PropertyShape(Name = "z")] public double m_z;
        [PropertyShape(Name = "r")] public byte m_r;

        public override string ToString()
        {
            return $"locID:{m_locID},ps:{(int)m_pose},x:{m_x},y:{m_y},z:{m_z},r:{m_r}";
        }
    }
}