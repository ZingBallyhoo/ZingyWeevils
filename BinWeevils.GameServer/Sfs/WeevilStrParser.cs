using StackXML.Str;

namespace BinWeevils.GameServer.Sfs
{
    public class WeevilStrParser : StandardStrParser
    {
        public static WeevilStrParser s_instance = new WeevilStrParser();

        public override T Parse<T>(ReadOnlySpan<char> span)
        {
            var val = base.Parse<T>(span);
            
            if (typeof(T) == typeof(float))
            {
                var floatVal = (float)(object)val;
                if (float.IsNaN(floatVal) || !float.IsFinite(floatVal))
                {
                    throw new InvalidDataException($"invalid float: {span}");
                }
            } else if (typeof(T) == typeof(double))
            {
                var doubleVal = (double)(object)val;
                if (double.IsNaN(doubleVal) || !double.IsFinite(doubleVal))
                {
                    throw new InvalidDataException($"invalid double: {span}");
                }
            }
            
            return val;
        }
    }
}