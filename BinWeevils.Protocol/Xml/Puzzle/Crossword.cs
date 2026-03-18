using System.Diagnostics;
using System.Text;
using StackXML;

namespace BinWeevils.Protocol.Xml.Puzzle
{
    [XmlCls("crossWord")]
    public partial class Crossword : PuzzleBase
    {
        [XmlField("heading")] public string m_heading;
        [XmlField("reward")] public uint m_reward = 400;
        
        [XmlBody] public List<PuzzleRow> m_rows;
        [XmlBody] public CrosswordClueListAcross m_across;
        [XmlBody] public CrosswordClueListDown m_down;
        
        public string GetSolutionText()
        {
            var sb = new StringBuilder();
            foreach (var row in m_rows)
            {
                sb.Append(row.m_text);
            }

            return sb.ToString();
        }
    }

    [XmlCls("across")]
    public partial class CrosswordClueListAcross
    {
        [XmlBody] public List<CrosswordClueAcross> m_clues;
    }
    
    [XmlCls("down")]
    public partial class CrosswordClueListDown
    {
        [XmlBody] public List<CrosswordClueDown> m_clues;
    }
    
    [XmlCls("clue")]
    public partial class CrosswordClueAcross
    {
        [XmlField("y")] public byte m_y;
        [XmlField("x1")] public byte m_x1;
        [XmlField("x2")] public byte m_x2;

        [XmlBody] public string m_text;
    }

    [XmlCls("clue")]
    public partial class CrosswordClueDown
    {
        [XmlField("x")] public byte m_x;
        [XmlField("y1")] public byte m_y1;
        [XmlField("y2")] public byte m_y2;

        [XmlBody] public string m_text;
    }
}