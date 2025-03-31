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
            options.InputFormatters.Insert(0, new FormInputFormatter());
            options.OutputFormatters.Insert(0, new FormOutputFormatter());
        });
        
        var app = builder.Build();

        app.UseRouting();

        app.MapStaticAssets();
        app.MapControllers();
        app.MapRazorPages().WithStaticAssets();
        
        var archivePath = app.Configuration["ArchivePath"]!;
        var playFs = InitArchiveStaticFiles(Path.Combine(archivePath, "play"), "");
        app.UseStaticFiles(playFs);

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

