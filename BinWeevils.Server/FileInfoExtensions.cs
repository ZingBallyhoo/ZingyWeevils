using Microsoft.Extensions.FileProviders;

namespace BinWeevils.Server
{
    public static class FileInfoExtensions
    {
        extension(IFileInfo fileInfo)
        {
            public string GetStringContents()
            {
                using var stream = fileInfo.CreateReadStream();
            
                using var streamReader = new StreamReader(stream);
                return streamReader.ReadToEnd();
            }

            public async Task<string> GetStringContentsAsync()
            {
                await using var stream = fileInfo.CreateReadStream();
            
                using var streamReader = new StreamReader(stream);
                return await streamReader.ReadToEndAsync();
            }
        }
    }
}