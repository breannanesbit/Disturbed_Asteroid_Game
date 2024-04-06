using Actors.UserActors;
using Akka.Actor;
using Microsoft.AspNetCore.SignalR;
using SignalRAPI.Hub;

namespace Actors
{
    public class SignalRActor : ReceiveActor
    {
        private IHubContext<ComunicationHub> _hubContext;

        public SignalRActor(IHubContext<ComunicationHub> hubContext)
        {
            _hubContext = hubContext;

            Receive<ChangeUserState>(message =>
            {
                _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
            });
        }
    }
}
