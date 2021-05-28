using System.IO;
using System.Net;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace WeevilWorld.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            
            services.AddSingleton<SocketHostService>();
            services.AddSingleton<IHostedService>(p => p.GetRequiredService<SocketHostService>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
            
            var cdn = CreateFS(@"D:\re\ww\serv_filesystem", "");
            app.UseStaticFiles(cdn);

            app.UseWebSockets();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path != "/ws")
                {
                    await next();
                    return;
                }

                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                    return;
                }
                
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var socket = new WebSocketInterface(webSocket, true);

                var hostService = context.RequestServices.GetRequiredService<SocketHostService>();
                var host = hostService.m_host;
                
                var hl = host.CreateHighLevelSocket(socket);
                await host.AddSocket(hl);

                var tcs = new TaskCompletionSource();
                socket.m_cancellationTokenSource.Token.Register(() => tcs.SetResult());
                await tcs.Task;
            });
        }
        
        private static StaticFileOptions CreateFS(string dir, string requestPath)
        {
            var fileProvider = new PhysicalFileProvider(dir);
            
            var options = new StaticFileOptions();
            options.ServeUnknownFileTypes = true;
            options.FileProvider = fileProvider;
            options.RequestPath = requestPath;
            options.OnPrepareResponse = context =>
            {
                var contentType = "application/octet-stream";
                
                var path = context.File.Name;
                var ext = Path.GetExtension(path);
                if (ext == ".swf")
                {
                    contentType = "application/x-shockwave-flash";
                } else if (ext == ".xml")
                {
                    contentType = "application/xml";
                } else if (ext == ".css")
                {
                    contentType = "text/css";
                }
                context.Context.Response.ContentType = contentType;
            };
            return options;
        }
    }
}
