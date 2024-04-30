using Akka.Actor;
using Shared;

namespace Actors.UserActors
{
    public class HeadSupervisor : ReceiveActor
    {
        private readonly IActorRef newLobbySupervisor;
        private readonly IActorRef newUserSupervisor;
        private readonly IActorRef _signalRActor;

        public HeadSupervisor(IActorRef newUserSupervisor, IActorRef newLobbySupervisor)
        {
            this.newUserSupervisor = newUserSupervisor;
            this.newLobbySupervisor = newLobbySupervisor;


            Receive<Lobby>((lobby) =>
            {
                newLobbySupervisor.Forward(lobby);
            });
            Receive<User>((user) =>
            {
                Console.WriteLine("in head");
                newUserSupervisor.Forward(user);
            });
            Receive<NewLobbyObject>((newLobby) =>
            {
                newUserSupervisor.Forward(newLobby);
            });
            Receive<ChangeGameState>((state) =>
            {
                newLobbySupervisor.Forward(state);
            });
            Receive<AddUserToLobby>((user) =>
            {
                newUserSupervisor.Forward(user);
            });
            Receive<AllLobbies>((lobbies) =>
            {
                newLobbySupervisor.Forward(lobbies);
            });
            Receive<MoveEvent>((MoveEvent) =>
            {
                newLobbySupervisor.Forward(MoveEvent);
            });
            Receive<Lazer>((lazer) =>
            {
                newLobbySupervisor.Forward(lazer);
            });
            Receive<StopActorMessage>((poison) =>
            {
                newLobbySupervisor.Forward(poison);
            });
            Receive<ShipUpdate>((shipUpdate) =>
            {
                newLobbySupervisor.Forward(shipUpdate);
            });
        }
    }

}
