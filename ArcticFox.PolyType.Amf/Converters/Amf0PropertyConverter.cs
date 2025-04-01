using PolyType.Abstractions;

namespace ArcticFox.PolyType.Amf.Converters
{
    public abstract class Amf0PropertyConverter<TDeclaringType>(string name)
    {
        public string Name { get; } = name;
        public abstract bool HasGetter { get; }
        public abstract bool HasSetter { get; }
        
        public abstract void Read(ref AmfDecoder decoder, ref TDeclaringType value);
        public abstract void Write(ref AmfEncoder encoder, ref TDeclaringType value);
    }
    
    public class Amf0PropertyConverter<TDeclaringType, TPropertyType> : Amf0PropertyConverter<TDeclaringType>
    {
        private readonly AmfConverter<TPropertyType> _propertyConverter;
        private readonly Getter<TDeclaringType, TPropertyType>? _getter;
        private readonly Setter<TDeclaringType, TPropertyType>? _setter;
        
        public Amf0PropertyConverter(IPropertyShape<TDeclaringType, TPropertyType> property, AmfConverter<TPropertyType> propertyConverter)
            : base(property.Name)
        {
            _propertyConverter = propertyConverter;

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

        public override void Read(ref AmfDecoder decoder, ref TDeclaringType declaringType)
        {
            var result = _propertyConverter.Read(ref decoder);
            _setter!(ref declaringType, result!);
        }

        public override void Write(ref AmfEncoder encoder, ref TDeclaringType declaringType)
        {
            var value = _getter!(ref declaringType);
            _propertyConverter.Write(ref encoder, value);
        }
    }
}