using Actors.UserActors;
using Akka.Actor;
using Shared.SignalRService;

namespace Actors
{
    public class SignalRActor : ReceiveActor
    {
        private readonly ActorSignalRService signalRService;

        public SignalRActor(ActorSignalRService signalRService)
        {
            this.signalRService = signalRService;

            Receive<GameLobby>(message =>
            {
                signalRService.SendGameLobby(message).PipeTo(
                    recipient: Self,
                    success: () =>
                    {
                        Console.WriteLine("success");
                        return "success";
                    },
                    failure: _ex =>
                    {
                        Console.WriteLine("failure");
                        return new SignalRErrorMessage
                        {
                            Message = $"error: {_ex.Message}"
                        };
                    });

                Console.WriteLine($"In signalR actor {message}");
                Console.WriteLine($"made it after");
            });

            Receive<GameState>(message =>
            {
                //will need to change this so only the players recieve the message for this lobby
                signalRService.SendGameState(message).PipeTo(
                    recipient: Self,
                    success: () =>
                    {
                        Console.WriteLine("success");
                        return "success";
                    },
                    failure: _ex =>
                    {
                        Console.WriteLine("failure");
                        return new SignalRErrorMessage
                        {
                            Message = $"error: {_ex.Message}"
                        };
                    });
                //_hubContext.Clients.All.SendAsync("StartGame", message);
            });

            Receive<AllLobbies>(message =>
            {
                signalRService.SendAllLobbies(message).PipeTo(
                    recipient: Self,
                    success: () =>
                    {
                        Console.WriteLine("success all lobbies");
                        return "success";
                    },
                    failure: _ex =>
                    {
                        Console.WriteLine("failure");
                        return new SignalRErrorMessage
                        {
                            Message = $"error: {_ex.Message}"
                        };
                    });
                //_hubContext.Clients.All.SendAsync("AllLobbiesSend", message);
            });
        }

        public class SignalRErrorMessage
        {
            public string Message { get; set; }
        }

    }
}
