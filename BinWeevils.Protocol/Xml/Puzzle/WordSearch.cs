using System.Text;
using BinWeevils.Protocol.Str;
using StackXML;

namespace BinWeevils.Protocol.Xml.Puzzle
{
    [XmlCls("wordSearch")]
    public partial class WordSearch : PuzzleBase
    {
        [XmlField("hideWords")] public bool m_hideWords;
        [XmlField("heading")] public string m_heading;

        [XmlBody] public List<PuzzleRow> m_rows;
        [XmlBody] public List<WordSearchWord> m_words; // todo: why can't this be list<string> with name specified :)

        public string ReadSpan(WordSearchSpan span)
        {
            var sb = new StringBuilder();

            foreach (var (i, j) in span.Enumerate())
            {
                sb.Append(m_rows[j].m_text[i]);
            }

            return sb.ToString();
        }

        public bool IsWord(string word)
        {
            // fine to do a basic reverse, will only be ascii
            var reversedWord = new string(word.Reverse().ToArray());
            
            return 
                m_words.Any(x => x.m_text.Equals(word, StringComparison.InvariantCultureIgnoreCase)) ||
                m_words.Any(x => x.m_text.Equals(reversedWord, StringComparison.InvariantCultureIgnoreCase));
        }
    }

    [XmlCls("word")]
    public partial class WordSearchWord
    {
        [XmlBody] public string m_text;
    }
}