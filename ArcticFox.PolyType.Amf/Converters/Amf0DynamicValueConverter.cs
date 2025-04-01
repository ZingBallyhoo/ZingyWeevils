using PolyType.Utilities;

namespace ArcticFox.PolyType.Amf.Converters
{
    public class Amf0DynamicValueConverter(TypeCache typeCache) : AmfConverter<object>
    {
        public override void Write(ref AmfEncoder encoder, object? value)
        {
            if (value == null)
            {
                encoder.PutMarker(Amf0TypeMarker.Null);
                return;
            }
            
            var shape = typeCache.Provider!.GetShape(value.GetType());
            var converter = AmfPolyType.GetConverter(shape!);
            if (converter is Amf0DynamicValueConverter or null)
            {
                throw new Exception($"unable to resolve converter for type: {shape}");
            }
            converter.WriteAsObject(ref encoder, value);
        }

        public override object? Read(ref AmfDecoder decoder)
        {
            var marker = decoder.ReadMarker();
            
            // todo: peek the marker instead? and handoff to child converter...

            switch (marker)
            {
                case Amf0TypeMarker.StrictArray:
                {
                    return ReadStrictArray(ref decoder);
                }
                case Amf0TypeMarker.TypedObject:
                {
                    var typeName = decoder.ReadUtf8();
                    break;
                }
            }
            
            throw new Exception($"unknown marker: {marker}");
        }
        
        private object? ReadStrictArray(ref AmfDecoder decoder)
        {
            var count = decoder.ReadUInt32();
            if (count > 10) throw new InvalidDataException(); // todo: configurable limit?
            
            var array = new object?[count];
            for (var i = 0; i < count; i++)
            {
                array[i] = Read(ref decoder);
            }
            return array;
        }
    }
}