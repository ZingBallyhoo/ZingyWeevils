namespace BinWeevils.GameServer.PolyType
{
    public class KeyValueConverter
    {
            
    }
        
    public abstract class KeyValueConverter<T> : KeyValueConverter
    {
        public abstract T Read(ReadOnlySpan<char> text);
    }
        
    public class StringKeyValueConverter : KeyValueConverter<string>
    {
        public override string Read(ReadOnlySpan<char> text)
        {
            return text.ToString();
        }
    }
        
    public class IntKeyValueConverter : KeyValueConverter<int>
    {
        public override int Read(ReadOnlySpan<char> text)
        {
            return int.Parse(text);
        }
    }
    
    public class DoubleKeyValueConverter : KeyValueConverter<double>
    {
        public override double Read(ReadOnlySpan<char> text)
        {
            return double.Parse(text);
        }
    }
    
    public class BoolKeyValueConverter : KeyValueConverter<bool>
    {
        public override bool Read(ReadOnlySpan<char> text)
        {
            return text switch
            {
                "true" => true,
                "false" => false,
                _ => throw new InvalidDataException($"unknown bool token: \"{text}\"")
            };
        }
    }
        
    public class ObjectKeyValueConverter<T>(Func<T> defaultConstructor, KeyValuePropertyConverter<T>[] properties) : KeyValueConverter<T>
    {
        private readonly KeyValuePropertyConverter<T>[] m_propertiesToRead = properties.Where(prop => prop.HasSetter).ToArray();
            
        public override T Read(ReadOnlySpan<char> text)
        {
            var inst = defaultConstructor();

            foreach (var varRange in text.Split(','))
            {
                var varSpan = text[varRange];
                if (varSpan.Length == 0) continue; // trailing comma
                
                var indexOfColon = varSpan.IndexOf(':');
                
                var nameSpan = varSpan.Slice(0, indexOfColon);
                var valueSpan = varSpan.Slice(indexOfColon+1);
                    
                var nameString = nameSpan.ToString();
                var property = m_propertiesToRead.SingleOrDefault(x => x.Name == nameString);
                if (property == null)
                {
                    throw new InvalidDataException($"{typeof(T)}: unknown field \"{nameSpan}\"");
                }
                    
                property.Read(valueSpan, ref inst);
            }
                
            return inst;
        }
    }
}