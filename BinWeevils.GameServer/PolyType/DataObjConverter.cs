using BinWeevils.Protocol.XmlMessages;

namespace BinWeevils.GameServer.PolyType
{
    public abstract class DataObjConverter
    {
        public abstract void AppendAsObject(ActionScriptObject obj, string? name, object? value);
    }

    public abstract class DataObjConverter<T> : DataObjConverter
    {
        public abstract void AppendToXml(ActionScriptObject obj, string? name, T? value);

        public override void AppendAsObject(ActionScriptObject obj, string? name, object? value)
        {
            AppendToXml(obj, name, (T?)value);
        }
    }
    
    public class DataObjObjectConverter<T>(DataObjPropertyConverter<T>[] properties) : DataObjConverter<T>
    {
        private readonly DataObjPropertyConverter<T>[] m_propertiesToWrite = properties.Where(prop => prop.HasGetter).ToArray();

        public override void AppendToXml(ActionScriptObject obj, string? name, T? value)
        {
            ArgumentNullException.ThrowIfNull(value);
            // todo: could null be allowed for sub objects?
            
            if (name == null)
            {
                // we are root
                AppendToXml(obj, value);
                return;
            }
            
            var subObject = new SubActionScriptObject
            {
                // "o": object
                // "a": array
                // original server uses "a" for login user object, actionscript doesnt care anyway
                m_type = "o",
                m_name = name
            };
            AppendToXml(subObject, value);
            obj.m_objects.Add(subObject);
        }
        
        private void AppendToXml(ActionScriptObject obj, T value)
        {
            foreach (var propertyConverter in m_propertiesToWrite)
            {
                propertyConverter.AppendToXml(obj, value);
            }
        }
    }
    
    public class DataObjBoolConverter : DataObjConverter<bool>
    {
        public override void AppendToXml(ActionScriptObject obj, string? name, bool value)
        {
            ArgumentNullException.ThrowIfNull(name);

            obj.m_vars.Add(Var.String(name, $"{(value ? "true" : "false")}"));
        }
    }
    
    public class DataObjStringConverter : DataObjConverter<string>
    {
        public override void AppendToXml(ActionScriptObject obj, string? name, string? value)
        {
            ArgumentNullException.ThrowIfNull(name);

            if (value == null)
            {
                obj.m_vars.Add(Var.Null(name));
                return;
            }
            obj.m_vars.Add(Var.String(name, value));
        }
    }
    
    public class DataObjIntConverter : DataObjConverter<int>
    {
        public override void AppendToXml(ActionScriptObject obj, string? name, int value)
        {
            ArgumentNullException.ThrowIfNull(name);
            
            obj.m_vars.Add(Var.Number(name, value));
        }
    }
}