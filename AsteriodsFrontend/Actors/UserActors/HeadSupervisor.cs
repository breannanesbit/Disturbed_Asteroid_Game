using Akka.Actor;

namespace Actors.UserActors
{
    public class HeadSupervisor : ReceiveActor
    {
        private readonly IActorRef newLobbySupervisor;
        private readonly IActorRef newUserSupervisor;

        public HeadSupervisor()
        {
            newLobbySupervisor = Context.ActorOf(LobbySupervisor.Props());
            newUserSupervisor = Context.ActorOf(UserSupervisor.Props());

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
