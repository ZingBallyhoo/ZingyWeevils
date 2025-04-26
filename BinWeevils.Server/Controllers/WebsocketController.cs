using ArcticFox.Net.Sockets;
using BinWeevils.GameServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    public class WebsocketController : Controller
    {
        private readonly BinWeevilsSocketHost m_socketHost;
        
        public WebsocketController(BinWeevilsSocketHost socketHost)
        {
            m_socketHost = socketHost;
        }
        
        [Route("/ws")]
        public async Task Get(CancellationToken cancellationToken)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var socket = new WebSocketInterface(webSocket, true);
            
            var hl = m_socketHost.CreateHighLevelSocket(socket);
            await m_socketHost.AddSocket(hl);

            var tcs = new TaskCompletionSource();
            socket.m_cancellationTokenSource.Token.Register(() => tcs.SetResult());
            await tcs.Task;
        }
    }
}