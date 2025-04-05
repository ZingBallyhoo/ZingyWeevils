using PolyType.Abstractions;

namespace BinWeevils.GameServer.PolyType
{
    public abstract class KeyValuePropertyConverter<TDeclaringType>(string name)
    {
        public string Name { get; } = name;
        public abstract bool HasGetter { get; }
        public abstract bool HasSetter { get; }
        
        public abstract void Read(ReadOnlySpan<char> text, ref TDeclaringType declaringType);
    }
    
    public class KeyValuePropertyConverter<TDeclaringType, TPropertyType> : KeyValuePropertyConverter<TDeclaringType>
    {
        private readonly KeyValueConverter<TPropertyType> m_propertyConverter;
        private readonly Getter<TDeclaringType, TPropertyType>? _getter;
        private readonly Setter<TDeclaringType, TPropertyType>? _setter;
        
        public KeyValuePropertyConverter(IPropertyShape<TDeclaringType, TPropertyType> property, KeyValueConverter<TPropertyType> propertyConverter)
            : base(property.Name)
        {
            m_propertyConverter = propertyConverter;

            if (property.HasGetter)
            {
                _getter = property.GetGetter();
            }

            if (property.HasSetter) 
            { 
                _setter = property.GetSetter();
            }
        }
        
        public override bool HasGetter => _getter != null;
        public override bool HasSetter => _setter != null;
            
        public override void Read(ReadOnlySpan<char> text, ref TDeclaringType declaringType)
        {
            var result = m_propertyConverter.Read(text);
            _setter!(ref declaringType, result!);
        }
    }
}