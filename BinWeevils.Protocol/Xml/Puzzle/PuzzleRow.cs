using StackXML;

namespace BinWeevils.Protocol.Xml.Puzzle
{
    [XmlCls("row")]
    public partial class PuzzleRow
    {
        [XmlField("id")] public byte m_id;
        [XmlBody] public string m_text;
    }
}