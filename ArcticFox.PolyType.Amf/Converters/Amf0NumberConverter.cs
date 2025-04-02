namespace ArcticFox.PolyType.Amf.Converters
{
    public class Amf0NumberConverter : AmfConverter<double>
    {
        public override void Write(ref AmfEncoder encoder, double value)
        {
            encoder.PutMarker(Amf0TypeMarker.Number);
            encoder.PutDouble(value);
        }

        public override double Read(ref AmfDecoder decoder)
        {
            var marker = decoder.ReadMarker();
            if (marker != Amf0TypeMarker.Number)
            {
                throw new NotImplementedException($"unknown number marker: {marker}");
            }
            
            return decoder.ReadDouble();
        }
    }
}