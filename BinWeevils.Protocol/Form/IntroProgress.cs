using System.Globalization;
using System.Text;
using ByteDev.FormUrlEncoded;
using CommunityToolkit.HighPerformance.Helpers;

namespace BinWeevils.Protocol.Form
{
    public class SetIntroProgressRequest
    {
        [FormUrlEncodedPropertyName("progress")] public string m_progress { get; set; }
    }
    
    public class GetIntroProgressResponse
    {
        [FormUrlEncodedPropertyName("res")] public string m_result { get; set; }
    }
    
    public enum IntroProgressBit
    {
        CLOSED_GARDEN_INVENTORY, // "10" - garden chest or haggle hut... lol
        CLOSED_ROOM_INVENTORY, // "9"
        CLICKED_ROOM_ITEM, // "8"
        HARVESTED_PLANT, // "7"
        CLICKED_PLANT, // "6"
        RETURNED_TO_NEST, // "5"
        VISITED_MALL, // "4"
        OPENED_MAP, // "3"
        ADDED_PLANT, // "2"
        ADDED_ROOM_ITEM, // "1"
        COUNT, // actually 10
    }
    
    public struct EncodedIntroProgress
    {
        public ushort m_bits;
        
        public EncodedIntroProgress(ushort bits)
        {
            m_bits = bits;
        }
        
        public EncodedIntroProgress(ReadOnlySpan<char> str)
        {
            if (str is "0")
            {
                m_bits = 0;
            } else if (str is "1")
            {
                m_bits = ushort.MaxValue;
            }
            
            if (str.Length != (int)IntroProgressBit.COUNT)
            {
                throw new InvalidDataException($"wrong length for intro progress bits: \"{str}\"");
            }
            
            m_bits = ushort.Parse(str, NumberStyles.BinaryNumber);
        }

        public string Describe()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < (int)IntroProgressBit.COUNT; i++)
            {
                if (!BitHelper.HasFlag(m_bits, i)) continue;
                sb.Append($"{(IntroProgressBit)i}, ");
            }
            return sb.ToString();
        }
        
        public override string ToString()
        {
            if (m_bits == 0) return "0";
            if (m_bits == ushort.MaxValue) return "1";
            
            // todo: roslyn isn't folding to a constant...
            return m_bits.ToString($"b{(int)IntroProgressBit.COUNT}");
        }
    }
}