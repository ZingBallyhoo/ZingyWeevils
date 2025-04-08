using ArcticFox.PolyType.Amf;
using ArcticFox.RPC.AmfGateway;
using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer;
using BinWeevils.Protocol.Amf;
using BinWeevils.Protocol.Xml;
using BinWeevils.Server;
using BinWeevils.Server.Controllers;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using StackXML;
using Stl.DependencyInjection;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorPages();
        builder.Services.AddControllers(options =>
        {
            options.InputFormatters.Add(new FormInputFormatter());
            options.OutputFormatters.Add(new FormOutputFormatter());
            options.OutputFormatters.Add(new StackXMLOutputFormatter());
        });
        builder.Services.AddSingleton<IAmfGatewayRouter, WeevilGatewayRouter>();
        
        builder.Services.AddSingleton<BinWeevilsSocketHost>();
        builder.Services.AddSingleton<IHostedService>(p => p.GetRequiredService<BinWeevilsSocketHost>());
        builder.Services.UseRegisterAttributeScanner().RegisterFrom(typeof(Zone).Assembly);
        builder.Services.AddSingleton<ISystemHandler, WeevilSystemHandler>();
        
        builder.Services.AddSingleton<LocationDefinitions>(p => 
            XmlReadBuffer.ReadStatic<LocationDefinitions>(File.ReadAllText(p.GetRequiredService<IConfiguration>()["LocationDefinitions"]!)));
        
        var app = builder.Build();

        app.UseRouting();

        app.MapStaticAssets();
        app.MapControllers();
        app.MapRazorPages().WithStaticAssets();
        app.UseWebSockets();
        
        var amfOptions = new AmfOptions();
        amfOptions.AddTypedObject<GetLoginDetailsResponse>();
        app.MapAmfGateway("php/amfphp/gateway.php", new AmfGatewaySettings
        {
            m_options = amfOptions,
            m_shapeProvider = GatewayShapeWitness.ShapeProvider,
            m_swallowExceptions = false,
        });
        
        var archivePath = app.Configuration["ArchivePath"]!;
        
        var playFs = InitArchiveStaticFiles(Path.Combine(archivePath, "play"), "");
        app.UseStaticFiles(playFs);
        
        var cdnFs = InitArchiveStaticFiles(Path.Combine(archivePath, ""), "/cdn");
        app.UseStaticFiles(cdnFs);
        var cdnFallbackFs = InitArchiveStaticFiles(Path.Combine(archivePath, "play"), "/cdn");
        app.UseStaticFiles(cdnFallbackFs);

        app.Start();
        
        if (app.Environment.IsDevelopment())
        {
            var server = app.Services.GetRequiredService<IServer>();
            var addressFeature = server.Features.GetRequiredFeature<IServerAddressesFeature>();
            var host = addressFeature.Addresses.SingleOrDefault() ?? "unknown";
            
            app.Logger.LogInformation("Ruffle Desktop CLI: {Cli}", string.Join(" ", [
                "ruffle.exe",
                $"{host}/main.swf",
                "--tcp-connections allow",
                "--dummy-external-interface"
            ]));
        }
        
        await app.WaitForShutdownAsync();
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

