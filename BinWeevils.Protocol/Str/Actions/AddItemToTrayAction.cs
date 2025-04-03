using StackXML.Str;

namespace BinWeevils.Protocol.Str.Actions
{
    public partial struct AddItemToTrayAction
    {
        [StrField] public int m_trayType;
        [StrField] public int m_itemType;
    }
}