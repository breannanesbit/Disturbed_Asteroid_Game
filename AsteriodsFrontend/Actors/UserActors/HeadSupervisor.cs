using Akka.Actor;

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
                newUserSupervisor.Forward(user);
            });
            Receive<NewLobbyObject>((newLobby) =>
            {
                newUserSupervisor.Forward(newLobby);
            });
        }
    }

}
