using System.Collections.Concurrent;
using BinWeevils.Protocol.Xml;
using StackXML;

namespace BinWeevils.Server
{
    public class ItemConfigRepository
    {
        private readonly string m_basePath;
        private readonly ConcurrentDictionary<string, ItemConfig> m_cache;
        
        public ItemConfigRepository(IConfiguration configuration)
        {
            m_basePath = Path.Combine(configuration["ArchivePath"]!, "users");
            m_cache = new ConcurrentDictionary<string, ItemConfig>();
        }
        
        public async Task<ItemConfig> GetConfig(string name)
        {
            if (m_cache.TryGetValue(name, out var config))
            {
                return config;
            }
            
            var path = Path.Combine(m_basePath, $"{name}.xml");
            config = XmlReadBuffer.ReadStatic<ItemConfig>(await File.ReadAllTextAsync(path));
            m_cache[name] = config;
            return config;
        }
    }
}