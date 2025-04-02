using System.Dynamic;

namespace ArcticFox.PolyType.Amf.Converters
{
    public class Amf0AnonymousObjectConverter(AmfConverter propertyConverter) : AmfConverter<ExpandoObject>
    {
        public override void Write(ref AmfEncoder encoder, ExpandoObject? value)
        {
            throw new NotImplementedException();
        }

        public override ExpandoObject? Read(ref AmfDecoder decoder)
        {
            var marker = decoder.ReadMarker();
            if (marker != Amf0TypeMarker.Object)
            {
                throw new NotImplementedException($"unknown anonymous object marker: {marker}");
            }
            
            var expando = new ExpandoObject();
            IDictionary<string, object?> dict = expando;

            while (true)
            {
                var propertyName = decoder.ReadUtf8();
                if (propertyName.Length == 0)
                {
                    var propertyValueMarker = decoder.ReadMarker();
                    if (propertyValueMarker != Amf0TypeMarker.ObjectEnd)
                    {
                        throw new InvalidDataException($"expected ObjectEnd, got {propertyValueMarker}");
                    }
                    break;
                }
                
                dict.Add(propertyName, propertyConverter.ReadAsObject(ref decoder));
            }
            
            return expando;
        }
    }
}