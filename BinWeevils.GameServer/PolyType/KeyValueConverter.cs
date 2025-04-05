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
                var property = m_propertiesToRead.Single(x => x.Name == nameString);
                    
                property.Read(valueSpan, ref inst);
            }
                
            return inst;
        }
    }
}