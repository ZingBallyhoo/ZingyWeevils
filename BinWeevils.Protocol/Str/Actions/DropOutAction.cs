using StackXML.Str;

namespace BinWeevils.Protocol.Str.Actions
{
    public partial struct DropOutAction
    {
        [StrField] public double m_xDest;
        [StrField] public double m_yDest;
        [StrField] public double m_zDest;
        [StrField] public int m_destLocID;
        [StrField] public int m_destDoorID;
    }
}