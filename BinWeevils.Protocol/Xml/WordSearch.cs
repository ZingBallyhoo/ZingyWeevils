using System.Text;
using BinWeevils.Protocol.Str;
using StackXML;

namespace BinWeevils.Protocol.Xml
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
    
    [XmlCls("wordSearch")]
    public partial class WordSearch : PuzzleBase
    {
        [XmlField("hideWords")] public bool m_hideWords;
        [XmlField("heading")] public string m_heading;

        [XmlBody] public List<WordSearchRow> m_rows;
        [XmlBody] public List<WordSearchWord> m_words; // todo: why can't this be list<string> with name specified :)

        public string ReadSpan(WordSearchSpan span)
        {
            // sanity. non-normalized here would be critical
            span.Normalize();
            
            var sb = new StringBuilder();
            
            for (int i = span.m_iStart; i <= span.m_iEnd; i++)
            {
                for (int j = span.m_jStart; j <= span.m_jEnd; j++)
                {
                    sb.Append(m_rows[j].m_text[i]);
                }
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

    [XmlCls("row")]
    public partial class WordSearchRow
    {
        [XmlField("id")] public byte m_id;
        [XmlBody] public string m_text;
    }

    [XmlCls("word")]
    public partial class WordSearchWord
    {
        [XmlBody] public string m_text;
    }
}