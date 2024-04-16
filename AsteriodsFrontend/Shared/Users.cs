using Akka.Actor;
using Shared;

namespace Actors.UserActors
{
    public enum UserState { Playing, NotPlaying }

    public class UsersActorInfo
    {
        public string Username { get; set; }
        public IActorRef ActorRef { get; set; }
    }

    public class Lobby
    {
        public Guid Id { get; set; }
        public User HeadPlayer { get; set; }
        public IActorRef ActorRef { get; set; }
        public List<User> Players { get; set; }
    }

    public class GameLobby
    {
        public Guid Id { get; set; }
        public User HeadPlayer { get; set; }
        public GameState CurrentState { get; set; }
        public List<User> Players { get; set; } = new List<User>();
    }


    public class User
    {
        public string Username { get; set; }
        public string Path { get; set; }
        public string hubConnection { get; set; }
        public Ship Ship { get; set; } = new();
    }

    public class NewLobbyObject
    {
        public string username { get; set; }
        public string hubConnection { get; set; }
    }

    public class ChangeUserState
    {
        public UserState ChangedState { get; set; }
    }

    public class ChangeGameState
    {
        public string user { get; set; }
        public GameState lobbyState { get; set; }
        public Guid lobbyId { get; set; }
    }


    public class CreatedLobby
    {
        public Guid LobbyId { get; set; }
    }

    public class AddUserToLobby
    {
        public string username { get; set; }
        public Guid lobbyId { get; set; }

    }

    public class AllLobbies
    {
        public List<Lobby> Lobbies { get; set; }
    }


    public class DecreaseUserHealth
    {
        public User User { get; set; }
        public int Damage { get; set; }
        public Guid LobbyId { get; set; }
    }


}