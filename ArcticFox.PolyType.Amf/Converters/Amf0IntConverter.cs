namespace ArcticFox.PolyType.Amf.Converters
{
    public class Amf0IntConverter : AmfConverter<int>
    {
        public override void Write(ref AmfEncoder encoder, int value)
        {
            encoder.PutMarker(Amf0TypeMarker.Number);
            encoder.PutDouble(value);
        }

        public override int Read(ref AmfDecoder decoder)
        {
            throw new NotImplementedException();
        }
    }
}