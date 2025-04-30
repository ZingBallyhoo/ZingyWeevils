using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial record struct GetZoneTimeResponse
    {
        [StrField] public string m_dateTime;
    }
}