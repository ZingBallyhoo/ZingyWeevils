using System.Collections.Concurrent;
using BinWeevils.Protocol.Xml;
using Microsoft.Extensions.FileProviders;
using StackXML;

namespace BinWeevils.Server.Services
{
    public class ItemConfigRepository
    {
        private readonly IFileProvider m_fileProvider;
        private readonly ConcurrentDictionary<string, ItemConfig> m_cache;
        
        public ItemConfigRepository(IFileProvider fileProvider)
        {
            m_fileProvider = fileProvider;
            m_cache = new ConcurrentDictionary<string, ItemConfig>();
        }
        
        public async Task<ItemConfig> GetConfig(string name)
        {
            if (m_cache.TryGetValue(name, out var config))
            {
                return config;
            }
            
            var configText = await GetConfigText(name);
            config = XmlReadBuffer.ReadStatic<ItemConfig>(configText);
            m_cache[name] = config;
            return config;
        }

        private async Task<string> GetConfigText(string name)
        {
            var fileInfo = m_fileProvider.GetFileInfo(Path.Combine("users", $"{name}.xml"));
            await using var stream = fileInfo.CreateReadStream();
            
            using var streamReader = new StreamReader(stream);
            return await streamReader.ReadToEndAsync();
        }
    }
}