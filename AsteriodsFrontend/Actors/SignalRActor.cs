using Akka.Actor;
using Microsoft.AspNet.SignalR;
using SignalRAPI.Hub;

namespace Actors
{
    public class SignalRActor : ReceiveActor
    {
        private IHubContext<ComunicationHub> _hubContext;

        public SignalRActor(IHubContext<ComunicationHub> hubContext)
        {
            _hubContext = hubContext;

            Receive<string>(message =>
            {
                _hubContext.Clients.All.SendMessage("ReceiveMessage", message);
            });
        }
    }
}
