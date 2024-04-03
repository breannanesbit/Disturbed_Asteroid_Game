using Microsoft.AspNetCore.SignalR;

namespace SignalRAPI.Hub
{
    public class ComunicationHub : DynamicHub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
