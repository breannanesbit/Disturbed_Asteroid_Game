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
        public List<User> DeadPlayers { get; set; } = new List<User>();
        public List<Asteroid> Asteroids { get; set; } = new List<Asteroid>();
        public List<Lazer> Lazers { get; set; } = new List<Lazer>();
        public List<PowerUp> PowerUps { get; set; } = new List<PowerUp>();
    }

    public class UpdateGame
    {
        public GameLobby lobby { get; set; }
    }
    public class ShipUpdate
    {
        public Guid lobbyId { get; set; }
        public User user { get; set; }
        public Ship Ship { get; set; }
    }

    public class User
    {
        public string Username { get; set; }
        public int Points { get; set; } = 0;
        public string Path { get; set; }
        public string hubConnection { get; set; }
        public Ship Ship { get; set; } = new();
    }
    public class MoveEvent
    {
        public ShipMoves ShipMoves { get; set; }
        public User user { get; set; }
        public Guid lobbyId { get; set; }
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
        public GameLobby lobby { get; set; }
        public Guid lobbyId { get; set; }
    }


    public class CreatedLobby
    {
        public Guid LobbyId { get; set; }
    }

    public class AddUserToLobby
    {
        public string username { get; set; }
        public string hubConnection { get; set; }
        public Guid lobbyId { get; set; }

    }

    public class AllLobbies
    {
        public List<Guid> LobbiesIds { get; set; }
        public string hubConnection { get; set; }

    }


    public class DecreaseUserHealth
    {
        public User User { get; set; }
        public int Damage { get; set; }
        public Guid LobbyId { get; set; }
    }

    public class UserInAGame
    {
        public User user { get; set; }
        public GameLobby Game { get; set; }
    }

    public class UserAndGameId
    {
        public User user { get; set; }
        public Guid LobbyId { get; set; }
    }



}