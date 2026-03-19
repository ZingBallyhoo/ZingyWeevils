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
            if (m_iStart != m_iEnd && m_jStart != m_jEnd)
            {
                if (m_iStart > m_iEnd)
                {
                    (m_iStart, m_iEnd) = (m_iEnd, m_iStart);
                    (m_jStart, m_jEnd) = (m_jEnd, m_jStart);
                }
                
                return;
            }

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

        public void Validate()
        {
            var deltaI = m_iEnd - m_iStart;
            var deltaJ = m_jEnd - m_jStart;

            var bothZero = deltaI == 0 && deltaJ == 0;
            if (bothZero)
            {
                throw new InvalidDataException("invalid word search span: both axes zero");
            }

            var eitherZero = deltaI == 0 || deltaJ == 0;
            if (eitherZero)
            {
                // cool
                return;
            }

            if (deltaI != deltaJ && deltaI != -deltaJ)
            {
                throw new InvalidDataException($"word search span is not a valid diagonal. i:{deltaI} j:{deltaJ}");
            }
        }

        public IEnumerable<(int, int)> Enumerate()
        {
            var iSign = Math.Sign(m_iEnd - m_iStart);
            var jSign = Math.Sign(m_jEnd - m_jStart);
            
            Debug.Assert(iSign != 0 || m_iStart == m_iEnd);
            Debug.Assert(jSign != 0 || m_jStart == m_jEnd);
            
            int i = m_iStart;
            int j = m_jStart;
            do
            {
                yield return (i, j);
                
                j += jSign;
                i += iSign;
            } while (j != m_jEnd || i != m_iEnd);
            
            yield return (i, j);
        }
    }
}