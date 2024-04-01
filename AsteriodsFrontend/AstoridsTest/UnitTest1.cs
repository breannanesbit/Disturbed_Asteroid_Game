using Actors;
using Akka.Actor;
using Akka.TestKit.Xunit2;

namespace AstoridsTest
{
    public class UnitTest1 : TestKit
    {


        [Fact]
        public void RouterActor_Should_Route_Message_To_Worker_Actors_from_routerActor()
        {
            var probe = CreateTestProbe();

            using var system = ActorSystem.Create("MyTestSystem");

            // Create an instance of the RouterActor
            var routerActor = system.ActorOf(Props.Create<RouterActor>(), "router");

            // Send a message to the router actor
            var message = "test key";
            routerActor.Tell(message);

            // Expect the message to be routed to a worker actor
            var response = ExpectMsg<string>(TimeSpan.FromSeconds(10)); // Increase timeout to 5 seconds

            // Assert that the response contains the actor's path
            Assert.StartsWith("akka://MyTestSystem/user/router/worker", response);
        }

        [Fact]
        public void RouterActor_Should_Route_Message_To_Worker_Actors()
        {
            var probe = CreateTestProbe();

            using var system = ActorSystem.Create("MyTestSystem");

            // Create an instance of the RouterActor
            var routerActor = system.ActorOf(Props.Create<Worker>(), "worker");

            // Send a message to the router actor
            var message = "test key";
            routerActor.Tell(message);

            // Expect the message to be routed to a worker actor
            var response = ExpectMsg<string>(TimeSpan.FromSeconds(5)); // Increase timeout to 5 seconds

            // Assert that the response contains the actor's path
            Assert.StartsWith("akka://MyTestSystem/user/worker", response);
        }
    }
}