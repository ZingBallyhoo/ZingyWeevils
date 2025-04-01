namespace ArcticFox.PolyType.Amf.Converters
{
    public class AmfHeaderConverter<TContent>(AmfConverter<TContent> contentConverter) : AmfConverter<AmfHeader>
    {
        public override void Write(ref AmfEncoder encoder, AmfHeader? value)
        {
        }

        public override AmfHeader? Read(ref AmfDecoder decoder)
        {
            // todo: can we limit the decoder to only the range specified by length?

            var name = decoder.ReadUtf8();
            var mustUnderstand = decoder.ReadBool();
            var length = decoder.ReadUInt32();
            var content = contentConverter.Read(ref decoder);
            
            return new AmfHeader
            {
                m_name = name,
                m_mustUnderstand = mustUnderstand,
                m_content = content
            };
        }
    }
}