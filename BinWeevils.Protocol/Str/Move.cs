using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial struct ClientMove
    {
        [StrField] public double m_x;
        [StrField] public double m_z;
        [StrField] public double m_dir;
    }
    
    public partial struct ServerMove
    {
        [StrField] public int m_uid;
        [StrField] public double m_x;
        [StrField] public double m_z;
        [StrField] public int m_dir;
    }
}