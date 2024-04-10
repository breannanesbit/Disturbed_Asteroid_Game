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


            UserSup.Forward(u);
            var response = probe.ExpectMsg<User>(TimeSpan.FromSeconds(5));

            Assert.Equal(response.Username, username);
            //Assert.StartsWith("akka://MyTestSystem/user/UserSupervisor/TomRiddle", response.Path);
        }
        /*public void AddUser_ToUserSupervisor()
        {
            var signalRProbe = CreateTestProbe();
            var newUserSupervisorProbe = CreateTestProbe();
            var newLobbySupervisorProbe = CreateTestProbe();

            using var Sys = ActorSystem.Create("MyTestSystem");

            var headSupervisor = Sys.ActorOf(Props.Create(() => new HeadSupervisor(newUserSupervisorProbe, newLobbySupervisorProbe)));
            var userSupervisor = Sys.ActorOf(Props.Create(() => new UserSupervisor(signalRProbe, newLobbySupervisorProbe)));
            var username = "testUser";
            headSupervisor.Tell(new User() { Username =username });
            var response = newUserSupervisorProbe.ExpectMsg<User>(TimeSpan.FromSeconds(5));
            Assert.Equal(response.Username, username);
            //Assert.NotNull(response.Path);
        }*/
        [Fact]
        public void AddLobby_ToLobbySupervisor()
        {
            var signalRProbe = CreateTestProbe();
            var newUserSupervisorProbe = CreateTestProbe();
            var newLobbySupervisorProbe = CreateTestProbe();

            using var Sys = ActorSystem.Create("MyTestSystem");

            var userSupervisor = Sys.ActorOf(Props.Create(() => new UserSupervisor(signalRProbe, newLobbySupervisorProbe)));
            var username = "testUser";
            var userActor = Sys.ActorOf(UserActor.Props(), username);

            userSupervisor.Tell(new NewLobbyObject { username = username });
            var newLobbyObject = new NewLobbyObject { username = username };

            var response = newLobbySupervisorProbe.ExpectMsg<NewLobbyObject>(TimeSpan.FromSeconds(5));  
            //Assert.Equal(username, response.username);
            //Assert.NotNull(response.Path);
        }
    }
}