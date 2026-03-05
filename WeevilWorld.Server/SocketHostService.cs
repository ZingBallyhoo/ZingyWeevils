using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;
using Microsoft.Extensions.Hosting;
using WeevilWorld.Server.Net;

namespace WeevilWorld.Server
{
    public class SocketHostService : IHostedLifecycleService
    {
        public readonly WeevilWorldSocketHost m_host;
        public readonly TcpServer m_tcpServer;
        
        public SocketHostService()
        {
            m_host = new WeevilWorldSocketHost();
            m_tcpServer = new TcpServer(m_host, new IPEndPoint(IPAddress.Loopback, 2110));
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await m_host.StartZone();
            
            await m_host.StartAsync(cancellationToken);
            m_tcpServer.StartAcceptWorker();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StartingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StartedAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task StoppingAsync(CancellationToken cancellationToken)
        {
            await m_host.StopAsync(cancellationToken);
        }

        public Task StoppedAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}