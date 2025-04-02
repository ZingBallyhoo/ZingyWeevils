using PolyType.Abstractions;

namespace ArcticFox.PolyType.Amf.Converters
{
    public class Amf0EnumerableConverter<TEnumerable, TElement>(
        AmfConverter<TElement> elementConverter, 
        IEnumerableTypeShape<TEnumerable, TElement> typeShape) : AmfConverter<TEnumerable>
    {
        public override void Write(ref AmfEncoder encoder, TEnumerable? value)
        {
            if (value == null)
            {
                encoder.PutMarker(Amf0TypeMarker.Null);
                return;
            }
            
            var enumerable = typeShape.GetGetEnumerable()(value);
            if (!enumerable.TryGetNonEnumeratedCount(out var count))
            {
                throw new NotSupportedException("can't serialize enumerables without a known count");
            }
            
            encoder.PutMarker(Amf0TypeMarker.StrictArray);
            encoder.PutInt32(count);

            foreach (var element in enumerable)
            {
                elementConverter.Write(ref encoder, element);
            }
        }

        public override TEnumerable? Read(ref AmfDecoder decoder)
        {
            var marker = decoder.ReadMarker();

            switch (marker)
            {
                case Amf0TypeMarker.StrictArray:
                {
                    return ReadStrictArray(ref decoder);
                }
                default:
                {
                    throw new NotImplementedException($"unknown enumerable marker: {marker}");
                }
            }
        }
        
        private TEnumerable? ReadStrictArray(ref AmfDecoder decoder)
        {
            var count = decoder.ReadUInt32();
            if (count > decoder.GetRemainingBytes())
            {
                // there must be at least 1 byte per array element
                throw new InvalidDataException("bad array length");
            }
            
            if (count > 10) throw new InvalidDataException(); // todo: configurable limit?
            
            var array = new TElement?[count];
            for (var i = 0; i < count; i++)
            {
                array[i] = elementConverter.Read(ref decoder);
            }
            
            return typeShape.GetSpanConstructor()(array);
        }
    }
}