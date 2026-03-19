using BinWeevils.Protocol.Str;

namespace BinWeevils.Tests
{
    public class PuzzleUnitTests
    {
        [Theory]
        [InlineData(0, 0, 15, 0)] // flat, i
        [InlineData(0, 0, 0, 15)] // flat, j
        [InlineData(0, 0, 15, 15)] // diagonal
        [InlineData(15, 15, 0, 0)] // diagonal
        [InlineData(15, 0, 0, 15)] // diagonal
        public void NormalizeSpan(byte iStart, byte jStart, byte iEnd, byte jEnd)
        {
            var normalized1 = new WordSearchSpan
            {
                m_iStart = iStart,
                m_iEnd = iEnd,
                m_jStart = jStart,
                m_jEnd = jEnd
            };
            normalized1.Normalize();
            
            var normalized2 = new WordSearchSpan
            {
                m_iStart = iEnd,
                m_iEnd = iStart,
                m_jStart = jEnd,
                m_jEnd = jStart
            };
            normalized2.Normalize();

            Assert.Equivalent(normalized1, normalized2, true);
            
            normalized1.Validate();
            normalized2.Validate();
        }
    }
}