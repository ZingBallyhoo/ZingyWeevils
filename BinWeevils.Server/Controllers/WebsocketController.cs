using Microsoft.AspNetCore.Mvc;

namespace BinWeevils.Server.Controllers
{
    public class WebsocketController : Controller
    {
        [Route("/ws")]
        public async Task Get(CancellationToken cancellationToken)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            
            using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
            while (!ws.CloseStatus.HasValue)
            {
                await ws.ReceiveAsync(new Memory<byte>(new byte[1]), cancellationToken);
            }
        }
    }
}