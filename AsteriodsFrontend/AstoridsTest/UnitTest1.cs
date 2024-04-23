using Actors;
using Actors.UserActors;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using Microsoft.Extensions.Logging;
using Shared.SignalRService;

namespace AstoridsTest
{
    public class UnitTest1 : TestKit
    {

        //[Fact]
        //public void RouterActor_Should_Route_Message_To_Worker_Actors()
        //{
        //    var probe = CreateTestProbe();

        //    using var system = ActorSystem.Create("MyTestSystem");

        //    // Create an instance of the RouterActor
        //    var routerActor = system.ActorOf(Props.Create<Worker>(), "worker");

        //    // Send a message to the router actor
        //    var message = "test key";
        //    routerActor.Tell(message);

        //    // Expect the message to be routed to a worker actor
        //    var response = ExpectMsg<string>(TimeSpan.FromSeconds(5)); // Increase timeout to 5 seconds

        //    // Assert that the response contains the actor's path
        //    Assert.StartsWith("akka://MyTestSystem/user/worker", response);
        //}

        [Fact]
        public void AddUser_ToUserSupervisor()
        {
            var probe = CreateTestProbe();
            using var system = ActorSystem.Create("MyTestSystem");

            var signalRActor = system.ActorOf(Props.Create<SignalRActor>(), "SignalRActor");
            var newLobbySupervisor = system.ActorOf(Props.Create<LobbySupervisor>(), "NewLobbySupervisor");
            var signalRService = new ActorSignalRService();

            var UserSup = system.ActorOf(Props.Create(() => new UserSupervisor(signalRService, newLobbySupervisor)), "UserSupervisor");

            var username = "TomRiddle";
            var u = new User() { Username = username };

            UserSup.Tell(u);
            var response = ExpectMsg<User>(TimeSpan.FromSeconds(5));

            //Assert.Equal(username, response.username);
            Assert.StartsWith("akka://MyTestSystem/user/UserSupervisor", response.Path);
            //Assert.NotNull(response.Path);
        }

        [Fact]
        public void CreateNewLobbyUsingLobbySupervisor()
        {
            var probe = CreateTestProbe();
            using var system = ActorSystem.Create("MyTestSystem");


            var lobbyActor = system.ActorOf(Props.Create<LobbyActor>(new LoggerFactory().CreateLogger<LobbyActor>()), "SignalRActor");


            var username = "TomRiddle";

            var user = new User() { Username = username, hubConnection = "test" };

            var lobby = new Lobby { HeadPlayer = user, ActorRef = lobbyActor, Id = Guid.NewGuid() };

            lobbyActor.Tell(lobby);
            var response = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            Assert.Equal(response.HeadPlayer.Username, username);
            Assert.Equal(response.CurrentState, GameState.Joining);
        }

        [Fact]
        public void ChangeGameStateUsingLobbySupervisor()
        {
            var probe = CreateTestProbe();
            using var system = ActorSystem.Create("MyTestSystem");


            var lobbyActor = system.ActorOf(Props.Create<LobbyActor>(new LoggerFactory().CreateLogger<LobbyActor>()), "SignalRActor");

            var username = "TomRiddle";

            var user = new User() { Username = username, hubConnection = "test" };

            var lobby = new Lobby { HeadPlayer = user, ActorRef = lobbyActor, Id = Guid.NewGuid() };

            lobbyActor.Tell(lobby);
            var response = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            var change = new ChangeGameState { lobbyId = lobby.Id, lobbyState = GameState.Playing, user = username };
            lobbyActor.Tell(change);

            var response2 = ExpectMsg<GameState>(TimeSpan.FromSeconds(5));

            Assert.Equal(response.HeadPlayer.Username, username);
            Assert.Equal(response2, GameState.Playing);
        }

        [Fact]
        public void ChangeUserState()
        {
            var probe = CreateTestProbe();
            using var system = ActorSystem.Create("MyTestSystem");


            var lobbyActor = system.ActorOf(Props.Create<UserActor>(new LoggerFactory().CreateLogger<LobbyActor>()), "SignalRActor");

            var username = "TomRiddle";

            var add = new AddUserToLobby { username = username, lobbyId = Guid.NewGuid() };
            lobbyActor.Tell(add);

            var response2 = ExpectMsg<GameState>(TimeSpan.FromSeconds(5));


            Assert.Equal(response2, GameState.Playing);
        }


        [Fact]
        public void AddMultipleUserToLobby()
        {
            var probe = CreateTestProbe();
            using var system = ActorSystem.Create("MyTestSystem");


            var lobbyActor = system.ActorOf(Props.Create<LobbyActor>(new LoggerFactory().CreateLogger<LobbyActor>()), "SignalRActor");


            var username = "TomRiddle";

            var user = new User() { Username = username, hubConnection = "test" };

            var lobby = new Lobby { HeadPlayer = user, ActorRef = lobbyActor, Id = Guid.NewGuid() };

            lobbyActor.Tell(lobby);
            var response = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            var newuser = new AddUserToLobby { username = "Test", lobbyId = lobby.Id };
            lobbyActor.Tell(newuser);

            var response2 = ExpectMsg<int>(TimeSpan.FromSeconds(5));

            Assert.Equal(response.HeadPlayer.Username, username);
            Assert.Equal(response2, 2);
        }

        [Fact]
        public void AddMultiplemoreUserToLobby()
        {
            var probe = CreateTestProbe();
            using var system = ActorSystem.Create("MyTestSystem");

            var lobbyActor = system.ActorOf(Props.Create<LobbyActor>(new LoggerFactory().CreateLogger<LobbyActor>()), "SignalRActor");

            var username = "TomRiddle";
            var user = new User() { Username = username, hubConnection = "test" };

            var lobby = new Lobby { HeadPlayer = user, ActorRef = lobbyActor, Id = Guid.NewGuid() };

            lobbyActor.Tell(lobby);
            var response = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            var newuser = new AddUserToLobby { username = "bob", lobbyId = lobby.Id };
            lobbyActor.Tell(newuser);

            var response2 = ExpectMsg<int>(TimeSpan.FromSeconds(5));

            var newuser2 = new AddUserToLobby { username = "sally", lobbyId = lobby.Id };
            lobbyActor.Tell(newuser2);

            var response3 = ExpectMsg<int>(TimeSpan.FromSeconds(5));

            Assert.Equal(response.HeadPlayer.Username, username);
            Assert.Equal(response3, 3);

        }

        [Fact]
        public void AddUser_ToLobbyUsingSupervisor()
        {
            var probe = CreateTestProbe();
            using var system = ActorSystem.Create("MyTestSystem");

            var signalRActor = system.ActorOf(Props.Create<SignalRActor>(), "SignalRActor");
            var newLobbySupervisor = system.ActorOf(Props.Create<LobbySupervisor>(signalRActor), "NewLobbySupervisor");


            var username = "TomRiddle";

            var u = new User() { Username = username };

            var newlobby = new NewLobbyObject { username = username };

            newLobbySupervisor.Tell(newlobby);
            //var response = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            //var newuser = new AddUserToLobby { username = username, lobbyId = response.Id };
            //newLobbySupervisor.Tell(newuser);

            //var response2 = ExpectMsg<int>(TimeSpan.FromSeconds(5));

            //Assert.Equal(response2, 2);
        }

        [Fact]
        public void GetAllTheLobbies()
        {
            using var system = ActorSystem.Create("MyTestSystem");


            var signalRActor = system.ActorOf(Props.Create<SignalRActor>(), "SignalRActor");

            var signalRService = new ActorSignalRService();

            var newLobbySupervisor = system.ActorOf(Props.Create<LobbySupervisor>(signalRService, new LoggerFactory().CreateLogger<LobbySupervisor>()), "NewLobbySupervisor");


            var username = "TomRiddle";
            var username2 = "Sally";
            var username3 = "Bobby";


            var u = new User() { Username = username, hubConnection = "test" };
            var u2 = new User() { Username = username2, hubConnection = "test" };
            var u3 = new User() { Username = username3, hubConnection = "test" };


            var newlobby = new NewLobbyObject { username = username };
            var newlobby2 = new NewLobbyObject { username = username2 };
            var newlobby3 = new NewLobbyObject { username = username3 };

            newLobbySupervisor.Tell(newlobby);
            newLobbySupervisor.Tell(newlobby2);
            newLobbySupervisor.Tell(newlobby3);

            var all = new AllLobbies();
            newLobbySupervisor.Tell(all);

            var response = ExpectMsg<List<Lobby>>(TimeSpan.FromSeconds(5));
            Assert.Equal(response.Count(), 3);

        }

        [Fact]
        public void TestUserCanTakeDamageForSupervisor()
        {
            using var system = ActorSystem.Create("MySystem");

            var signalRActor = system.ActorOf(Props.Create<SignalRActor>(), "SignalRActor");
            var signalRService = new ActorSignalRService();

            var newLobbySupervisor = system.ActorOf(Props.Create<LobbySupervisor>(signalRService, new LoggerFactory().CreateLogger<LobbySupervisor>()), "NewLobbySupervisor");

            var username = "TomRiddle";

            var user = new User() { Username = username, hubConnection = "test" };

            var newlobby = new NewLobbyObject { username = username, hubConnection = "hubConnection" };

            newLobbySupervisor.Tell(newlobby);

            var response = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            var duh = new DecreaseUserHealth()
            {
                LobbyId = response.Id,
                User = user,
                Damage = 10,
            };

            newLobbySupervisor.Tell(duh);
            var response2 = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            Assert.Equal(response2?.HeadPlayer.Ship.Health, 90);

        }

        [Fact]
        public void TestUsersCanTakeDamage()
        {

            using var system = ActorSystem.Create("MyTestSystem1");

            var lobbyActor = system.ActorOf(Props.Create<LobbyActor>(new LoggerFactory().CreateLogger<LobbyActor>()), "SignalRActor");

            var username = "TomRiddle";
            var user = new User() { Username = username, hubConnection = "test" };

            var lobby = new Lobby { HeadPlayer = user, ActorRef = lobbyActor, Id = Guid.NewGuid() };

            lobbyActor.Tell(lobby);
            var response = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            var duh = new DecreaseUserHealth()
            {
                LobbyId = response.Id,
                User = user,
                Damage = 10,
            };

            lobbyActor.Tell(duh);
            var response2 = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            Assert.Equal(response2?.HeadPlayer.Ship.Health, 90);
        }

        [Fact]
        public void TestGameOver()
        {
            using var system = ActorSystem.Create("MyTestSystem");

            var lobbyActor = system.ActorOf(Props.Create<LobbyActor>(new LoggerFactory().CreateLogger<LobbyActor>()), "SignalRActor");

            var username = "TomRiddle";
            var user = new User() { Username = username, hubConnection = "test" };

            var lobby = new Lobby { HeadPlayer = user, ActorRef = lobbyActor, Id = Guid.NewGuid() };

            lobbyActor.Tell(lobby);
            var response = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            var duh = new DecreaseUserHealth()
            {
                LobbyId = response.Id,
                User = user,
                Damage = 100,
            };

            lobbyActor.Tell(duh);
            var response2 = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            Assert.Equal(response2.HeadPlayer.Ship.Health, 0);
            Assert.Equal(response2.CurrentState, GameState.Over);

        }

        [Fact]
        public void TestGameOverFromTheSupervisor()
        {
            using var system = ActorSystem.Create("MyTestSystem");

            var signalRActor = system.ActorOf(Props.Create<SignalRActor>(), "SignalRActor");
            var signalRService = new ActorSignalRService();

            var newLobbySupervisor = system.ActorOf(Props.Create<LobbySupervisor>(signalRService, new LoggerFactory().CreateLogger<LobbySupervisor>()), "NewLobbySupervisor");

            var username = "TomRiddle";

            var user = new User() { Username = username, hubConnection = "test" };

            var newlobby = new NewLobbyObject { username = username, hubConnection = "hubConnection" };

            newLobbySupervisor.Tell(newlobby);

            var response = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            var duh = new DecreaseUserHealth()
            {
                LobbyId = response.Id,
                User = user,
                Damage = 100,
            };

            newLobbySupervisor.Tell(duh);
            var response2 = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            Assert.Equal(response2?.HeadPlayer.Ship.Health, 0);
            Assert.Equal(response2.CurrentState, GameState.Over);
        }
    }
}