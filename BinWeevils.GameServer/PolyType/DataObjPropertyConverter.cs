using BinWeevils.Protocol.XmlMessages;
using PolyType.Abstractions;

namespace BinWeevils.GameServer.PolyType
{
    public abstract class DataObjPropertyConverter<TDeclaringType>(string name)
    {
        public string Name { get; } = name;
        public abstract bool HasGetter { get; }
        public abstract bool HasSetter { get; }
        
        public abstract void AppendToXml(ActionScriptObject obj, TDeclaringType declaringType);
    }
    
    public class DataObjPropertyConverter<TDeclaringType, TPropertyType> : DataObjPropertyConverter<TDeclaringType>
    {
        private readonly DataObjConverter<TPropertyType> m_propertyConverter;
        private readonly Getter<TDeclaringType, TPropertyType>? _getter;
        private readonly Setter<TDeclaringType, TPropertyType>? _setter;
        
        public DataObjPropertyConverter(IPropertyShape<TDeclaringType, TPropertyType> property, DataObjConverter<TPropertyType> propertyConverter)
            : base(property.Name)
        {
            m_propertyConverter = propertyConverter;

            if (property.HasGetter)
            {
                _getter = property.GetGetter();
            }

            if (property.HasSetter) 
            { 
                _setter = property.GetSetter();
            }
        }
        
        public override bool HasGetter => _getter != null;
        public override bool HasSetter => _setter != null;

        public override void AppendToXml(ActionScriptObject obj, TDeclaringType declaringType)
        {
            var value = _getter!(ref declaringType);
            m_propertyConverter.AppendToXml(obj, Name, value);
        }
    }
}