using System.Dynamic;
using ArcticFox.Codec.Binary;
using ArcticFox.PolyType.Amf.Converters;
using PolyType;
using PolyType.Abstractions;
using PolyType.Utilities;

namespace ArcticFox.PolyType.Amf
{
    public static class AmfPolyType
    {
        private static readonly MultiProviderTypeCache s_converterCaches = new MultiProviderTypeCache
        {
            ValueBuilderFactory = ctx => new Builder(ctx)
            // todo: delay
        };
        
        public static AmfConverter GetConverter(ITypeShape typeShape)
        {
            return (AmfConverter)s_converterCaches.GetOrAdd(typeShape)!;
        }
        
        public static byte[] Serialize<T, TProvider>(T value) where TProvider : IShapeable<T>
        {
            var converter = (AmfConverter<T>)GetConverter(TProvider.GetShape());
            
            var encoder = new AmfEncoder();
            try
            {
                converter.Write(ref encoder, value);
                return encoder.m_writer.GetData().ToArray();
            } finally
            {
                encoder.Dispose();
            }
        }
        
        public static byte[] Serialize<T>(T value) where T : IShapeable<T>
        {
            return Serialize<T, T>(value);
        }
        
        public static T Deserialize<T, TProvider>(scoped ReadOnlySpan<byte> data) where TProvider : IShapeable<T>
        {
            var converter = (AmfConverter<T>)GetConverter(TProvider.GetShape())!;
            
            var decoder = new AmfDecoder(new BitReader(data));
            return converter.Read(ref decoder)!;
        }
        
        public static T Deserialize<T>(scoped ReadOnlySpan<byte> data) where T : IShapeable<T>
        {
            return Deserialize<T, T>(data);
        }
        
        private sealed class Builder(TypeGenerationContext generationContext) : TypeShapeVisitor, ITypeShapeFunc
        {
            private ITypeShapeFunc self => generationContext;
            
            public AmfConverter<T> GetOrAddConverter<T>(ITypeShape<T> typeShape) =>
                (AmfConverter<T>)self.Invoke(typeShape, this)!;
            
            public object? Invoke<T>(ITypeShape<T> typeShape, object? state = null)
            {
                return typeShape.Accept(this);
            }
        
            public override object? VisitObject<T>(IObjectTypeShape<T> type, object? state)
            {
                // todo: the way this works is a little wonky
                // can we allow core types without them being witnessed?
                
                if (type.Type == typeof(AmfPacket))
                {
                    var dyn = new Amf0DynamicValueConverter(generationContext.ParentCache!);
                    return new AmfPacketConverter(
                        new AmfHeaderConverter<object>(dyn),
                        new AmfMessageConverter<object>(dyn));
                }
                
                if (type.Type == typeof(object))
                {
                    return new Amf0DynamicValueConverter(generationContext.ParentCache!);
                }
                if (type.Type == typeof(string)) return new Amf0StringConverter();
                if (type.Type == typeof(int)) return new Amf0IntConverter();
                if (type.Type == typeof(double)) return new Amf0NumberConverter();
                
                if (type.Type.IsPrimitive)
                {
                    throw new Exception($"unimplemented primitive type: {type}");
                }
                
                var properties = type.Properties
                    .Select(prop => (Amf0PropertyConverter<T>)prop.Accept(this)!)
                    .ToArray();;
                return new Amf0TypedObjectConverter<T>(properties);
            }

            public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
            {
                var propertyConverter = GetOrAddConverter(propertyShape.PropertyType);
                return new Amf0PropertyConverter<TDeclaringType, TPropertyType>(propertyShape, propertyConverter);
            }
            
            public override object? VisitEnumerable<TEnumerable, TElement>(IEnumerableTypeShape<TEnumerable, TElement> enumerableShape, object? state)
            {
                var elementConverter = GetOrAddConverter(enumerableShape.ElementType);
                return new Amf0EnumerableConverter<TEnumerable, TElement>(elementConverter, enumerableShape);
            }

            public override object? VisitDictionary<TDictionary, TKey, TValue>(IDictionaryTypeShape<TDictionary, TKey, TValue> dictionaryShape, object? state = null)
            {
                // (ExpandoObject only)
                return new Amf0AnonymousObjectConverter(new Amf0DynamicValueConverter(generationContext.ParentCache!));
            }
        }
    }
}