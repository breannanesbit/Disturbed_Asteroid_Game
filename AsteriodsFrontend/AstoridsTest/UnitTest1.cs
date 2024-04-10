using Actors;
using Actors.UserActors;
using Akka.Actor;
using Akka.TestKit.Xunit2;

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

            var UserSup = system.ActorOf(Props.Create(() => new UserSupervisor(signalRActor, newLobbySupervisor)), "UserSupervisor");

            var username = "TomRiddle";
            var u = new User() { Username = username };

            UserSup.Tell(u);
            var response = probe.ExpectMsg<User>(TimeSpan.FromSeconds(5));

            //Assert.Equal(username, response.username);
            //Assert.NotNull(response.Path);
        }

        [Fact]
        public void CreateNewLobbyUsingLobbySupervisor()
        {
            var probe = CreateTestProbe();
            using var system = ActorSystem.Create("MyTestSystem");


            var lobbyActor = system.ActorOf(Props.Create<LobbyActor>(), "SignalRActor");


            var username = "TomRiddle";

            var lobby = new Lobby { HeadPlayer = username, ActorRef = lobbyActor, Id = Guid.NewGuid() };

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


            var lobbyActor = system.ActorOf(Props.Create<LobbyActor>(), "SignalRActor");

            var username = "TomRiddle";

            var lobby = new Lobby { HeadPlayer = username, ActorRef = lobbyActor, Id = Guid.NewGuid() };

            lobbyActor.Tell(lobby);
            var response = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            var change = new ChangeGameState { lobbyId = lobby.Id, lobbyState = GameState.Playing, user = username };
            lobbyActor.Tell(change);

            var response2 = probe.ExpectMsg<GameState>(TimeSpan.FromSeconds(5));

            Assert.Equal(response.HeadPlayer.Username, username);
            Assert.Equal(response2, GameState.Playing);
        }

        [Fact]
        public void ChangeUserState()
        {
            var probe = CreateTestProbe();
            using var system = ActorSystem.Create("MyTestSystem");


            var lobbyActor = system.ActorOf(Props.Create<UserActor>(), "SignalRActor");

            var username = "TomRiddle";

            var add = new AddUserToLobby { username = username, lobbyId = Guid.NewGuid() };
            lobbyActor.Tell(add);

            var response2 = probe.ExpectMsg<UserState>(TimeSpan.FromSeconds(5));


            Assert.Equal(response2, UserState.Playing);
        }


        [Fact]
        public void AddMultipleUserToLobby()
        {
            var probe = CreateTestProbe();
            using var system = ActorSystem.Create("MyTestSystem");


            var lobbyActor = system.ActorOf(Props.Create<LobbyActor>(), "SignalRActor");


            var username = "TomRiddle";
            var lobby = new Lobby { HeadPlayer = username, ActorRef = lobbyActor, Id = Guid.NewGuid() };

            lobbyActor.Tell(lobby);
            var response = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            var newuser = new AddUserToLobby { username = username, lobbyId = lobby.Id };
            lobbyActor.Tell(newuser);

            var response2 = probe.ExpectMsg<int>(TimeSpan.FromSeconds(5));

            Assert.Equal(response.HeadPlayer.Username, username);
            Assert.Equal(response2, 2);
        }

        public void AddMultiplemoreUserToLobby()
        {
            var probe = CreateTestProbe();
            using var system = ActorSystem.Create("MyTestSystem");


            var lobbyActor = system.ActorOf(Props.Create<LobbyActor>(), "SignalRActor");


            var username = "TomRiddle";
            var lobby = new Lobby { HeadPlayer = username, ActorRef = lobbyActor, Id = Guid.NewGuid() };

            lobbyActor.Tell(lobby);
            var response = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

            var newuser = new AddUserToLobby { username = "bob", lobbyId = lobby.Id };
            lobbyActor.Tell(newuser);

            var response2 = probe.ExpectMsg<int>(TimeSpan.FromSeconds(5));

            var newuser2 = new AddUserToLobby { username = "sally", lobbyId = lobby.Id };
            lobbyActor.Tell(newuser2);

            var response3 = probe.ExpectMsg<int>(TimeSpan.FromSeconds(5));

            Assert.Equal(response.HeadPlayer.Username, username);
            Assert.Equal(response3, 3);

        }

        public void AddUser_ToLobbyUsingSupervisor()
        {
            var probe = CreateTestProbe();
            using var system = ActorSystem.Create("MyTestSystem");

            var signalRActor = system.ActorOf(Props.Create<SignalRActor>(), "SignalRActor");
            var newLobbySupervisor = system.ActorOf(Props.Create<LobbySupervisor>(), "NewLobbySupervisor");


            var username = "TomRiddle";

            var u = new User() { Username = username };

            var newlobby = new NewLobbyObject { username = username };

            newLobbySupervisor.Tell(newlobby);
            var response = probe.ExpectMsg<Lobby>(TimeSpan.FromSeconds(5));

            var newuser = new AddUserToLobby { username = username, lobbyId = lobby.Id };
            newLobbySupervisor.Tell(newuser);

            var response2 = probe.ExpectMsg<int>(TimeSpan.FromSeconds(5));

            Assert.Equal(response2, 2);

            //Assert.Equal(username, response.username);
            //Assert.NotNull(response.Path);
        }
    }
}