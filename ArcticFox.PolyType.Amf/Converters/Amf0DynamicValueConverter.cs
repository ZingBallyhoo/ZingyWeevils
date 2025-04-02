using System.Dynamic;
using PolyType.Abstractions;
using PolyType.Utilities;

namespace ArcticFox.PolyType.Amf.Converters
{
    public class Amf0DynamicValueConverter(TypeCache typeCache) : AmfConverter<object>
    {
        private AmfConverter ResolveConverter(Type type)
        {
            var shape = typeCache.Provider!.Resolve(type);
            var converter = AmfPolyType.GetConverter(shape);
            if (converter is Amf0DynamicValueConverter or null)
            {
                throw new Exception($"unable to resolve converter for type: {shape}");
            }
            
            return converter;
        }
        
        public override void Write(ref AmfEncoder encoder, object? value)
        {
            if (value == null)
            {
                encoder.PutMarker(Amf0TypeMarker.Null);
                return;
            }
            
            var converter = ResolveConverter(value.GetType());
            converter.WriteAsObject(ref encoder, value);
        }

        public override object? Read(ref AmfDecoder decoder)
        {
            var peekDecoder = decoder; // copy
            var peekedMarker = peekDecoder.ReadMarker();

            switch (peekedMarker)
            {
                case Amf0TypeMarker.String:
                case Amf0TypeMarker.LongString:
                {
                    return ResolveConverter(typeof(string)).ReadAsObject(ref decoder);
                }
                case Amf0TypeMarker.StrictArray:
                case Amf0TypeMarker.EcmaArray:
                {
                    return ResolveConverter(typeof(object[])).ReadAsObject(ref decoder);
                }
                case Amf0TypeMarker.Number:
                {
                    return ResolveConverter(typeof(double)).ReadAsObject(ref decoder);
                }
                case Amf0TypeMarker.Object:
                {
                    return ResolveConverter(typeof(ExpandoObject)).ReadAsObject(ref decoder);
                }
            }
            
            throw new Exception($"unknown marker: {peekedMarker}");
        }
    }
}