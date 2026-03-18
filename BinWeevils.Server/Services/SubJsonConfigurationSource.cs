namespace BinWeevils.Server.Services
{
    public class SubJsonConfigurationSource : FileConfigurationSource
    {
        public string Prefix { get; set; }
        
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new SubJsonConfigurationProvider(this);
        }
    }
    
    public class SubJsonConfigurationProvider : FileConfigurationProvider
    {
        private readonly SubJsonConfigurationSource m_source;
        
        public SubJsonConfigurationProvider(SubJsonConfigurationSource source) : base(source)
        {
            m_source = source;
        }

        public override void Load(Stream stream)
        {
            var originalData = SubJsonConfigurationFileParser.Parse(stream);
            
            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in originalData)
            {
                Data[$"{m_source.Prefix}:{pair.Key}"] = pair.Value;
            }
        }
    }

    public static class SubJsonConfigurationExtensions
    {
        public static IConfigurationBuilder AddSubJson(this IConfigurationBuilder builder, string prefix, string path, bool optional = true, bool reloadOnChange = true)
        {
            return builder.Add<SubJsonConfigurationSource>(s =>
            {
                s.Prefix = prefix;
                
                s.Path = path;
                s.Optional = optional;
                s.ReloadOnChange = reloadOnChange;
                s.ResolveFileProvider();
            });
        }
    }
}