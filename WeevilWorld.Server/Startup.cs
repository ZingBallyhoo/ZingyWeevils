using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

namespace WeevilWorld.Server
{
    public class Startup
    {
        public const string c_domain = "ww.zingy.dev";
        
        public readonly IConfiguration m_configuration;

        public Startup(IConfiguration configuration)
        {
            m_configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            
            services.AddSingleton<SocketHostService>();
            services.AddSingleton<IHostedService>(p => p.GetRequiredService<SocketHostService>());
            
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.AllowedHosts = new List<string> { c_domain };
            });
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
                app.UseForwardedHeaders();
                
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
            
            // todo: pls let me serve files with no extension without this?
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                OnPrepareResponse = ctx =>
                {
                    var typedHeaders = ctx.Context.Response.GetTypedHeaders();
                    typedHeaders.CacheControl = new CacheControlHeaderValue
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromDays(365)
                    };
                    typedHeaders.Expires = new DateTimeOffset(DateTime.UtcNow).AddDays(365);
                }
            });

            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(15),
            };
            if (env.IsProduction()) webSocketOptions.AllowedOrigins.Add($"https://{c_domain}");
            app.UseWebSockets(webSocketOptions);
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
    }
}
