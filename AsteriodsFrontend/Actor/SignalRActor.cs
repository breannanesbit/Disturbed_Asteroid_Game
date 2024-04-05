using Akka.Actor;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using SignalRAPI.Hub;

namespace Actors
{
    public class SignalRActor : ReceiveActor
    {
        private HubConnection hubConnection1;

        public SignalRActor(IHubContext<ComunicationHub> hubContext)
        {
            hubConnection1 = new HubConnectionBuilder()
            .WithUrl("http://localhost:32772/ComunicationHub")
                    .WithAutomaticReconnect()
                    .Build();
            Receive<string>(message =>
            {
                hubConnection1.SendAsync("SendMessage", Sender.ToString(), message);
            });
        }
    }
}
