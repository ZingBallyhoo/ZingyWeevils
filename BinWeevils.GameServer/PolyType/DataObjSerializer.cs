using BinWeevils.Protocol.DataObj;
using BinWeevils.Protocol.XmlMessages;
using PolyType;
using PolyType.Abstractions;
using PolyType.Utilities;

namespace BinWeevils.GameServer.PolyType
{
    public static class DataObjSerializer
    {
        private static readonly MultiProviderTypeCache m_cache = new MultiProviderTypeCache
        {
            ValueBuilderFactory = ctx => new Builder(ctx)
            // todo: delay
        };
        
        private class Builder(TypeGenerationContext generationContext) : TypeShapeVisitor, ITypeShapeFunc
        {
            private ITypeShapeFunc self => generationContext;

            private DataObjConverter<T> ReEnter<T>(ITypeShape<T> typeShape)
            {
                return (DataObjConverter<T>)self.Invoke(typeShape)!;
            }

            public object? Invoke<T>(ITypeShape<T> typeShape, object? state = null)
            {
                return typeShape.Accept(this);
            }
            
            public override object? VisitObject<T>(IObjectTypeShape<T> type, object? state)
            {
                if (type.Type == typeof(string))
                {
                    return new DataObjStringConverter();
                }
                if (type.Type == typeof(int))
                {
                    return new DataObjIntConverter();
                }
                if (type.Type == typeof(bool))
                {
                    return new DataObjBoolConverter();
                }
                if (type.Type == typeof(double))
                {
                    return new DataObjDoubleConverter();
                }
                
                if (type.Type.IsPrimitive)
                {
                    throw new NotImplementedException($"unimplemented primitive: {type}");
                }
                
                var properties = type.Properties
                    .Select(prop => (DataObjPropertyConverter<T>)prop.Accept(this)!)
                    .ToArray();
                
                var inlineAttr = type.AttributeProvider?.GetCustomAttribute<DataObjInlineAttribute>();
                if (inlineAttr != null)
                {
                    return new DataObjInlineObjConverter<T>(properties);
                }
                return new DataObjObjectConverter<T>(properties);
            }
        
            public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
            {
                var propertyConverter = ReEnter(propertyShape.PropertyType);
                return new DataObjPropertyConverter<TDeclaringType, TPropertyType>(propertyShape, propertyConverter);
            }

            public override object? VisitOptional<TOptional, TElement>(IOptionalTypeShape<TOptional, TElement> optionalShape, object? state = null)
            {
                var underlyingConverter = ReEnter(optionalShape.ElementType);
                return new DataObjOptionalConverter<TOptional, TElement>
                {
                    m_innerConverter = underlyingConverter,
                    m_deconstructor = optionalShape.GetDeconstructor()
                };
            }
        }
        
        // todo: set up this way because we are passing in subclasses...
        
        public static ActionScriptObject ToXml<T>(T data) where T : IShapeable<T>
        {
            return ToXml(data, T.GetShape().Provider);
        }
        
        private static ActionScriptObject ToXml(object data, ITypeShapeProvider provider)
        {
            var output = new ActionScriptObject();
            var converter = (DataObjConverter)m_cache.GetOrAdd(data.GetType(), provider)!;
            converter.AppendAsObject(output, null, data);
            return output;
        }
    }
}