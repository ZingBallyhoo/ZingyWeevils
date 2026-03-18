using StackXML;

namespace BinWeevils.Protocol.Xml.Puzzle
{
    [XmlCls(null)]
    public abstract partial class PuzzleBase
    {
        [XmlField("id")] public byte m_id;
        
        public virtual ReadOnlySpan<char> GetNodeName()
        {
            throw new NotImplementedException();
        }
    }
}