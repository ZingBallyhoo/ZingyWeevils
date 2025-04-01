namespace ArcticFox.PolyType.Amf.Converters
{
    public class Amf0TypedObjectConverter<T>(Amf0PropertyConverter<T>[] properties) : AmfConverter<T>
    {
        private readonly Amf0PropertyConverter<T>[] m_propertiesToWrite = properties.Where(prop => prop.HasGetter).ToArray();
        
        public override void Write(ref AmfEncoder encoder, T? value)
        {
            encoder.PutMarker(Amf0TypeMarker.TypedObject);
            encoder.PutUtf8(typeof(T).Name); // todo: shape?

            foreach (var propertyConverter in m_propertiesToWrite)
            {
                encoder.PutUtf8(propertyConverter.Name);
                propertyConverter.Write(ref encoder, ref value);
            }
            
            encoder.PutUtf8("");
            encoder.PutMarker(Amf0TypeMarker.ObjectEnd);
        }

        public override T? Read(ref AmfDecoder decoder)
        {
            var marker = decoder.ReadMarker();
            if (marker != Amf0TypeMarker.TypedObject)
            {
                throw new InvalidDataException($"expected TypedObject marker, got {marker}");
            }
            
            throw new NotImplementedException();
        }
    }
}