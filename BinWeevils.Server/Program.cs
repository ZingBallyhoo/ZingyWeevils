using ArcticFox.PolyType.Amf;
using ArcticFox.RPC.AmfGateway;
using ArcticFox.SmartFoxServer;
using BinWeevils.Database;
using BinWeevils.GameServer;
using BinWeevils.Protocol.Amf;
using BinWeevils.Protocol.Xml;
using BinWeevils.Server;
using BinWeevils.Server.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
        
        builder.Services.AddDbContext<WeevilDBContext>(options =>
            options.UseSqlite("Filename=db.sqlite"));
        builder.Services.AddIdentity<WeevilAccount, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<WeevilDBContext>()
            .AddDefaultTokenProviders();
        
        builder.Services
            .AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, UsernameOnlyAuthenticationHandler>("UsernameOnly", _ => { });
        builder.Services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("UsernameOnly")
                .Build());
        
        var app = builder.Build();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllers();
        app.MapRazorPages().WithStaticAssets();
        app.UseWebSockets();
        
        var amfOptions = new AmfOptions();
        amfOptions.AddTypedObject<GetLoginDetailsResponse>();
        app.MapAmfGateway("api/php/amfphp/gateway.php", new AmfGatewaySettings
        {
            m_options = amfOptions,
            m_shapeProvider = GatewayShapeWitness.ShapeProvider,
            m_swallowExceptions = false,
        }).RequireAuthorization();
        
        // internally redirect legacy requests
        app.UseRewriter(new RewriteOptions()
            .AddRedirect("^assetsTycoon/VODroom\\.swf", "cdn/users/VODroom4.swf") // vodRoom
            .AddRedirect("bwtv/menu3\\.swf", "bwtv/menu5.swf") // vodRoom
            .AddRedirect("^bwtv/player_cs3_2\\.swf", "bwtv/player_cs3_15.swf")
            .AddRewrite("^bwtv/(.+)", "cdn/bwtv/$1", true) // vodRoom
            .AddRewrite("^cinema/lobbyScreenData\\.xml", "cdn/binConfig/uk/lobbyScreenData.xml", true) // riggs
            .AddRewrite("^cinema/cinema\\.xml", "cdn/externalUIs/cinema/cinema.xml", true) // riggs
            .AddRedirect("^cinema/(.+).flv", "cdn/ads/binweevils/binTycoonTour2.flv") // riggs
            .AddRewrite("^nestNews/xml/uk/nestNews\\.xml", "nestNews/xml/nestNews.xml", true)
        
            // overwritten with incompatible versions...
            .AddRewrite("^externalUIs/petBuilder\\.swf", "cdn/play/externalUIs/petBuilder.swf", true)
            .AddRewrite("^overlayUIs/introGuide\\.swf", "cdn/play/overlayUIs/introGuide.swf", true)
        );
        
        var archivePath = app.Configuration["ArchivePath"]!;
        
        // play stuff is older than root stuff
        var playFallbackFs = InitArchiveStaticFiles(Path.Combine(archivePath, ""), "");
        app.UseStaticFiles(playFallbackFs);
        var playFs = InitArchiveStaticFiles(Path.Combine(archivePath, "play"), "");
        app.UseStaticFiles(playFs);
        
        var cdnFs = InitArchiveStaticFiles(Path.Combine(archivePath, ""), "/cdn");
        app.UseStaticFiles(cdnFs);
        var cdnFallbackFs = InitArchiveStaticFiles(Path.Combine(archivePath, "play"), "/cdn");
        app.UseStaticFiles(cdnFallbackFs);
        
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<WeevilDBContext>();
            await context.Database.EnsureDeletedAsync(); // reset
            await context.Database.EnsureCreatedAsync();
            
            var sql = await File.ReadAllTextAsync(Path.Combine(archivePath, "..", "other", "itemType.sql"));
            await context.Database.ExecuteSqlRawAsync(sql);
        }

        await app.StartAsync();
        
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

