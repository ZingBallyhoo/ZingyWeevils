using PolyType;
using PolyType.Abstractions;
using PolyType.Utilities;

namespace BinWeevils.GameServer.PolyType
{
    public static class KeyValueDeserializer
    {
        private static readonly MultiProviderTypeCache m_cache = new MultiProviderTypeCache
        {
            ValueBuilderFactory = ctx => new Builder(ctx)
            // todo: delay
        };
        
        private class Builder(TypeGenerationContext generationContext) : TypeShapeVisitor, ITypeShapeFunc
        {
            private ITypeShapeFunc self => generationContext;

            private KeyValueConverter<T> ReEnter<T>(ITypeShape<T> typeShape)
            {
                return (KeyValueConverter<T>)self.Invoke(typeShape)!;
            }

            public object? Invoke<T>(ITypeShape<T> typeShape, object? state = null)
            {
                return typeShape.Accept(this);
            }
            
            public override object? VisitObject<T>(IObjectTypeShape<T> type, object? state)
            {
                if (type.Type == typeof(string))
                {
                    return new StringKeyValueConverter();
                }
                if (type.Type == typeof(int))
                {
                    return new IntKeyValueConverter();
                }
                
                var ctorShape = (IConstructorShape<T, object>)type.Constructor!;
                var properties = type.Properties
                    .Select(prop => (KeyValuePropertyConverter<T>)prop.Accept(this)!)
                    .ToArray();
                return new ObjectKeyValueConverter<T>(ctorShape.GetDefaultConstructor(), properties);
            }
        
            public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
            {
                var propertyConverter = ReEnter(propertyShape.PropertyType);
                return new KeyValuePropertyConverter<TDeclaringType, TPropertyType>(propertyShape, propertyConverter);
            }
        }
        
        public static T Deserialize<T>(ReadOnlySpan<char> text) where T : IShapeable<T>
        {
            var converter = (KeyValueConverter<T>)m_cache.GetOrAdd(T.GetShape())!;
            return converter.Read(text);
        }
    }
}