using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;
using Microsoft.Extensions.Hosting;

namespace WeevilWorld.Server
{
    public class SocketHostService : IHostedService
    {
        public readonly Net.WeevilWorldSocketHost m_host;
        public readonly TcpServer m_tcpServer;
        
        public SocketHostService()
        {
            m_host = new Net.WeevilWorldSocketHost();
            m_tcpServer = new TcpServer(m_host, new IPEndPoint(IPAddress.Loopback, 2110));
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await m_host.StartAsync();
            m_tcpServer.StartAcceptWorker();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await m_host.StopAsync();
        }
    }
}