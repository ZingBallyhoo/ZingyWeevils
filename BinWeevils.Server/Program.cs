using System.Threading.RateLimiting;
using ArcticFox.PolyType.Amf;
using ArcticFox.RPC.AmfGateway;
using ArcticFox.SmartFoxServer;
using BinWeevils.Common;
using BinWeevils.Common.Database;
using BinWeevils.GameServer;
using BinWeevils.Protocol.Amf;
using BinWeevils.Protocol.Xml;
using BinWeevils.Server;
using BinWeevils.Server.Controllers;
using BinWeevils.Server.Services;
using Grafana.OpenTelemetry;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Proto;
using Proto.DependencyInjection;
using StackXML;
using Stl.DependencyInjection;
using WeevilWorld.Server;

public class Program
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
        builder.Services.AddTransient<PetAmfService>();
        builder.Services.AddTransient<WeevilKartAmfService>();
        
        builder.Services.AddSingleton<BinWeevilsSocketHost>();
        builder.Services.AddSingleton<IHostedService>(p => p.GetRequiredService<BinWeevilsSocketHost>());
        builder.Services.UseRegisterAttributeScanner().RegisterFrom(typeof(Zone).Assembly);
        builder.Services.AddSingleton<ISystemHandler, WeevilSystemHandler>();
        builder.Services.AddSingleton(provider =>
        {
            Log.SetLoggerFactory(provider.GetRequiredService<ILoggerFactory>());
            return new ActorSystem().WithServiceProvider(provider);
        });
        builder.Services.AddSingleton<LocNameMapper>();
        builder.Services.AddScoped<WeevilInitializer>();
        builder.Services.AddScoped<PetInitializer>();
        builder.Services.AddSingleton<TimeProvider, UkTimeProvider>();
        
        builder.Services.AddSingleton<LocationDefinitions>(p => 
            XmlReadBuffer.ReadStatic<LocationDefinitions>(File.ReadAllText(p.GetRequiredService<IConfiguration>()["LocationDefinitions"]!)));
        builder.Services.AddSingleton<NestLocationDefinitions>(p => 
            XmlReadBuffer.ReadStatic<NestLocationDefinitions>(File.ReadAllText(p.GetRequiredService<IConfiguration>()["NestLocationDefinitions"]!)));
        builder.Services.AddSingleton<ItemConfigRepository>();
        builder.Services.AddSingleton<QuestRepository>();
        builder.Services.AddSingleton<TrackRepository>();
        builder.Services.AddOptions<DatabaseSettings>().BindConfiguration("Database");
        builder.Services.AddOptions<EconomySettings>();
        builder.Services.AddOptions<PetsSettings>().BindConfiguration("Pets").Validate(settings =>
        {
            return settings.ItemColors.Count == settings.BowlItemTypes.Count;
        }).ValidateOnStart();
        builder.Services.AddOptions<WeevilWheelsSettings>().BindConfiguration("WeevilWheels").Validate(settings => 
        {
            return true;
        }).ValidateOnStart();
        builder.Services.AddOptions<SinglePlayerGamesSettings>().BindConfiguration("SinglePlayerGames").Validate(settings => 
        {
            return true;
        }).ValidateOnStart();
        
        var connectionString = builder.Configuration.GetRequiredSection("Database")["ConnectionString"];
        builder.Services.AddDbContext<WeevilDBContext>(options =>
        {
            options.UseSqlite(connectionString);
        });
        builder.Services.AddScoped<DatabaseSeeding>();
        builder.Services.AddIdentity<WeevilAccount, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_ ";
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
        
        builder.Services.AddRateLimiter(options =>
        {
            options.AddPolicy<string>("Standard", context => 
            {
                var name = context.User.Identity?.Name;
                if (name == null) return RateLimitPartition.GetNoLimiter("no-auth");
                
                return RateLimitPartition.GetFixedWindowLimiter(name,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 3,
                        Window = TimeSpan.FromSeconds(1),
                        QueueLimit = 5
                    });
            });
        });
        
        var enableObservability = builder.Configuration.GetRequiredSection("Observability").GetValue<bool>("Enabled");
        if (enableObservability)
        {
            // for some reason otel causes slow shutdowns in tests... so needs to be disabled
            ConfigureObservability(builder);
        }
        
        await using var app = builder.Build();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

        app.MapStaticAssets().Finally(DisableTracing);
        app.MapControllers().RequireRateLimiting("Standard");
        app.MapRazorPages().WithStaticAssets().Finally(DisableTracing);
        app.UseWebSockets(new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromSeconds(15),
            KeepAliveTimeout = TimeSpan.FromSeconds(15)
        });
        
        var amfOptions = new AmfOptions();
        amfOptions.AddTypedObject<GetLoginDetailsResponse>();
        amfOptions.AddTypedObject<SubmitLapTimesResponse>();
        amfOptions.AddTypedObject<SubmitLapTimesResponse.MedalInfo>();
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
            .AddRewrite("^nestNews/xml/h/nestNews\\.xml", "nestNews/xml/nestNews.xml", true)
            .AddRewrite("^sounds/nestSounds/firePlace\\.mp3", "bintunes/flemManor/firePlace.mp3", true) // library_no_book
            .AddRedirect("^externalUIs/myMusicManager\\.swf", "externalUIs/myMusicManager_20_05_13.swf") // fix page size
            .AddRedirect("^overlayUIs/haggleHutOverlay\\.swf", "overlayUIs/haggleHutOverlay_18_02_2011.swf") // first version to support delimited colors
        
            // overwritten with incompatible versions...
            .AddRewrite("^externalUIs/petBuilder\\.swf", "cdn/play/externalUIs/petBuilder.swf", true)
            .AddRewrite("^overlayUIs/introGuide\\.swf", "cdn/play/overlayUIs/introGuide.swf", true)
            .AddRewrite("^fixedCam/shoppingMall_dynamAds\\.swf", "cdn/play/fixedCam/shoppingMall_dynamAds.swf", true) // bugged...
            .AddRewrite("^fixedCam/LabsLab\\.swf", "cdn/play/fixedCam/LabsLab.swf", true) // overwritten with videoPod
            
            // aspnet won't match the route otherwise...
            .AddRedirect("^api//nest/get-all-stored", "api/nest/get-all-stored")
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
            var seeder = services.GetRequiredService<DatabaseSeeding>();
            await seeder.Seed();
        }

        await app.StartAsync();
        
        if (app.Environment.IsDevelopment())
        {
            var server = app.Services.GetRequiredService<IServer>();
            var host = server.GetLocalHostingAddress() ?? "unknown";
            
            app.Logger.LogInformation("Ruffle Desktop CLI: {Cli}", string.Join(" ", [
                "ruffle.exe",
                $"{host}/main.swf",
                "--tcp-connections allow",
                "--dummy-external-interface",
                "-Pcluster=h",
                "--cookie",
                "username=desktop"
            ]));
        }
        
        await app.WaitForShutdownAsync();
    }
    
    private static void ConfigureObservability(WebApplicationBuilder builder)
    {
        builder.Services
            .AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddSource(GameServerObservability.s_source.Name);
                tracing.AddSource(ApiServerObservability.s_source.Name);
                tracing.UseGrafana();
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddMeter(GameServerObservability.s_meter.Name);
                metrics.AddMeter(ApiServerObservability.s_meter.Name);
                metrics.UseGrafana();
            });
        builder.Logging
            .AddOpenTelemetry(logging =>
            {
                logging.UseGrafana();
            });
    }
    
    private static void DisableTracing(EndpointBuilder b)
    {
        var original = b.RequestDelegate!;
        b.RequestDelegate = context =>
        {
            var activityFeature = context.Features.Get<IHttpActivityFeature>();
            if (activityFeature?.Activity is {} activity)
            {
                // don't trace
                activity.IsAllDataRequested = false;
            }

            return original(context);
        };
    }

    private static StaticFileOptions InitArchiveStaticFiles(string dir, string requestPath)
    {
        var fileProvider = new PhysicalFileProvider(dir);
            
        var options = new StaticFileOptions
        {
            ServeUnknownFileTypes = true,
            FileProvider = fileProvider,
            RequestPath = requestPath,
            
            DefaultContentType = "application/octet-stream",
            OnPrepareResponse = ctx =>
            {
                ctx.CacheResponse(TimeSpan.FromDays(365));

                var activityFeature = ctx.Context.Features.Get<IHttpActivityFeature>();
                if (activityFeature?.Activity is {} activity)
                {
                    // don't trace
                    activity.IsAllDataRequested = false;
                }
            }
        };
        return options;
    }
}

