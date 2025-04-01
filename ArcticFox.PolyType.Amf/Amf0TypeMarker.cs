namespace ArcticFox.PolyType.Amf
{
    public enum Amf0TypeMarker : byte
    {
        Number = 0,
        Boolean = 1,
        String = 2,
        Object = 3,
        MovieClip = 4, // not supported in spec
        Null = 5,
        Undefined = 6,
        Reference = 7,
        EcmaArray = 8,
        ObjectEnd = 9,
        StrictArray = 10,
        Date = 11,
        LongString = 12,
        Unsupported = 13,
        RecordSet = 14, // not supported in spec
        XmlDocument = 15,
        TypedObject = 16,
        AvmPlusObject = 17
    }
}