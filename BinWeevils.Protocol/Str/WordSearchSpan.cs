using System.Diagnostics;
using StackXML.Str;

namespace BinWeevils.Protocol.Str
{
    public partial struct WordSearchSpan
    {
        [StrField] public byte m_iStart;
        [StrField] public byte m_jStart;
        [StrField] public byte m_iEnd;
        [StrField] public byte m_jEnd;

        public void Normalize()
        {
            if (m_iStart > m_iEnd)
            {
                (m_iStart, m_iEnd) = (m_iEnd, m_iStart);
            }
            if (m_jStart > m_jEnd)
            {
                (m_jStart, m_jEnd) = (m_jEnd, m_jStart);
            }

            Debug.Assert(m_iStart <= m_iEnd);
            Debug.Assert(m_jStart <= m_jEnd);
        }
    }
}