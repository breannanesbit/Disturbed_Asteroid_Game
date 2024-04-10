using Actors;
using Actors.UserActors;
using Akka.Actor;
using Akka.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using SignalRAPI.Hub;
//using Microsoft.AspNet.SignalR;
//using Microsoft.AspNetCore.SignalR;
//using SignalRAPI.Hub;

namespace Akka.AspNetCore
{
    public class AkkaService : IHostedService, IActorBridge
    {
        private ActorSystem _actorSystem;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private IActorRef _actorRef;

        private readonly IHostApplicationLifetime _applicationLifetime;

        public AkkaService(IServiceProvider serviceProvider, IHostApplicationLifetime appLifetime, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _applicationLifetime = appLifetime;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var bootstrap = BootstrapSetup.Create();
            var diSetup = DependencyResolverSetup.Create(_serviceProvider);
            var actorSystemSetup = bootstrap.And(diSetup);

            _actorSystem = ActorSystem.Create("akka-universe", actorSystemSetup);

             //Create router actor instead of a single worker actor
             var signalRProps = Props.Create(() => new SignalRActor(
              _serviceProvider.GetRequiredService<IHubContext<ComunicationHub>>()));


            var signalRActorRef = _actorSystem.ActorOf(signalRProps, "signalRActor");

            var lobbySupervisorProps = Props.Create(() => new LobbySupervisor(signalRActorRef));
            var newLobbySupervisor = _actorSystem.ActorOf(lobbySupervisorProps);

            // Create the UserSupervisor actor with a reference to the SignalR actor
            var userSupervisorProps = Props.Create(() => new UserSupervisor(signalRActorRef, newLobbySupervisor));
            var newUserSupervisor = _actorSystem.ActorOf(userSupervisorProps);

            // Create the HeadSupervisor actor with a reference to the SignalR actor
            var headSupervisorProps = Props.Create(() => new HeadSupervisor(newUserSupervisor, newLobbySupervisor));
            _actorRef = _actorSystem.ActorOf(headSupervisorProps, "router");


            _actorSystem.WhenTerminated.ContinueWith(_ =>
            {
                _applicationLifetime.StopApplication();
            });

            await Task.CompletedTask;
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // strictly speaking this may not be necessary - terminating the ActorSystem would also work
            // but this call guarantees that the shutdown of the cluster is graceful regardless
            await CoordinatedShutdown.Get(_actorSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
        }

        public void Tell(object message)
        {
            _actorRef.Tell(message);
        }

        public Task<T> Ask<T>(object message)
        {
            return _actorRef.Ask<T>(message);
        }
    }
}
