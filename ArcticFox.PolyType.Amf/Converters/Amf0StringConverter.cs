namespace ArcticFox.PolyType.Amf.Converters
{
    public class Amf0StringConverter : AmfConverter<string>
    {
        public override void Write(ref AmfEncoder encoder, string? value)
        {
            if (value == null)
            {
                // todo: is this desired?
                encoder.PutMarker(Amf0TypeMarker.Null);
                return;
            }
            
            encoder.PutMarker(Amf0TypeMarker.String);
            encoder.PutUtf8(value);
        }

        public override string? Read(ref AmfDecoder decoder)
        {
            var marker = decoder.ReadMarker();

            switch (marker)
            {
                case Amf0TypeMarker.String:
                {
                    return decoder.ReadUtf8();
                }
                default:
                {
                    throw new NotImplementedException($"unknown string marker: {marker}");
                }
            }
        }
    }
}