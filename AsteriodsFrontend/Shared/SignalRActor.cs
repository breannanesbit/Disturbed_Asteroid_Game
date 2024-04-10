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
                _hubContext.Clients.All.SendAsync("SendMessage", message);
            });

            Receive<GameState>(message =>
            {
                //will need to change this so only the players recieve the message for this lobby
                _hubContext.Clients.All.SendAsync("StartGame", message);
            });
        }
    }
}
