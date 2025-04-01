using BinWeevils.Server;
using Microsoft.Extensions.FileProviders;

internal static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorPages();
        builder.Services.AddControllers(options =>
        {
            options.InputFormatters.Add(new FormInputFormatter());
            options.OutputFormatters.Add(new FormOutputFormatter());
            options.OutputFormatters.Add(new StackXMLOutputFormatter());
        });
        
        var app = builder.Build();

        app.UseRouting();

        app.MapStaticAssets();
        app.MapControllers();
        app.MapRazorPages().WithStaticAssets();
        app.UseWebSockets();
        
        var archivePath = app.Configuration["ArchivePath"]!;
        
        var playFs = InitArchiveStaticFiles(Path.Combine(archivePath, "play"), "");
        app.UseStaticFiles(playFs);
        
        var cdnFs = InitArchiveStaticFiles(Path.Combine(archivePath, ""), "/cdn");
        app.UseStaticFiles(cdnFs);
        var cdnFallbackFs = InitArchiveStaticFiles(Path.Combine(archivePath, "play"), "/cdn");
        app.UseStaticFiles(cdnFallbackFs);

        app.Run();
    }
    
    private static StaticFileOptions InitArchiveStaticFiles(string dir, string requestPath)
    {
        var fileProvider = new PhysicalFileProvider(dir);
            
        var options = new StaticFileOptions
        {
            ServeUnknownFileTypes = true,
            FileProvider = fileProvider,
            RequestPath = requestPath,
            
            DefaultContentType = "application/octet-stream"
        };
        return options;
    }
}

