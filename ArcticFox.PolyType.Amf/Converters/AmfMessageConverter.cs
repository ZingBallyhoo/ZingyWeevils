namespace ArcticFox.PolyType.Amf.Converters
{
    public class AmfMessageConverter<TBody>(AmfConverter<TBody> bodyConverter) : AmfConverter<AmfMessage>
    {
        public override void Write(ref AmfEncoder encoder, AmfMessage? value)
        {
            encoder.PutUtf8(value.m_targetUri);
            encoder.PutUtf8(value.m_responseUri);
            encoder.PutInt32(-1);
            bodyConverter.WriteAsObject(ref encoder, value.m_data);
        }

        public override AmfMessage? Read(ref AmfDecoder decoder)
        {
            // todo: can we limit the decoder to only the range specified by length?

            var targetUri = decoder.ReadUtf8();
            var responseUri = decoder.ReadUtf8();
            var messageLength = decoder.ReadUInt32();
            var body = bodyConverter.Read(ref decoder);
            
            return new AmfMessage
            {
                m_targetUri = targetUri,
                m_responseUri = responseUri,
                m_data = body
            };
        }
    }
}