using Actors;
using Actors.UserActors;
using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using Akka.Configuration;
using Akka.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Shared.SignalRService;

namespace Shared
{
    public class AkkaService : IHostedService, IActorBridge
    {
        private ActorSystem _actorSystem;
        private readonly IConfiguration _configuration;
        private readonly ActorSignalRService signalRService;
        private readonly IServiceProvider _serviceProvider;
        private IActorRef _actorRef;
        private IActorRef userInstance;
        private IActorRef lobbyInstance;


        private readonly IHostApplicationLifetime _applicationLifetime;

        public AkkaService(IServiceProvider serviceProvider, IHostApplicationLifetime appLifetime, IConfiguration configuration, ActorSignalRService signalRService)
        {
            _serviceProvider = serviceProvider;
            _applicationLifetime = appLifetime;
            _configuration = configuration;
            this.signalRService = signalRService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var diSetup = DependencyResolverSetup.Create(_serviceProvider);

            var clusterEnv = Environment.GetEnvironmentVariable("AKKA_CLUSTER");
            Console.WriteLine(clusterEnv);
            var config = ConfigurationFactory.ParseString(clusterEnv);

            var bootstrap = BootstrapSetup.Create().WithConfig(config);
            var actorSystemSetup = bootstrap.And(diSetup);

            _actorSystem = ActorSystem.Create("akka-universe", actorSystemSetup);

            var cluster = Akka.Cluster.Cluster.Get(_actorSystem);

            var signalRProps = Props.Create(() => new SignalRActor(signalRService));
            var signalRActorRef = _actorSystem.ActorOf(signalRProps, "signalRActor");

            if (cluster.SelfRoles.Contains("lobby"))
            {

                var lobbySupProps = DependencyResolver.For(_actorSystem).Props<LobbySupervisor>(signalRActorRef);
                var singletonProps = ClusterSingletonManager.Props(
                    singletonProps: lobbySupProps,
                    terminationMessage: PoisonPill.Instance,
                    settings: ClusterSingletonManagerSettings.Create(_actorSystem).WithRole("lobby"));
                lobbyInstance = _actorSystem.ActorOf(singletonProps, "lobbiesSingletonManager");
            }

            var proxyProps = ClusterSingletonProxy.Props(
                    singletonManagerPath: "user/lobbiesSingletonManager",
                    settings: ClusterSingletonProxySettings.Create(_actorSystem).WithRole("lobby"));
            var lobbySupervisorRef = _actorSystem.ActorOf(proxyProps, "lobbySupervisorProxy");


            if (cluster.SelfRoles.Contains("userSession"))
            {
                var userSupProps = DependencyResolver.For(_actorSystem).Props<UserSupervisor>(signalRActorRef, lobbySupervisorRef);
                var userSingletionProps = ClusterSingletonManager.Props(
                    singletonProps: userSupProps,
                    terminationMessage: PoisonPill.Instance,
                    settings: ClusterSingletonManagerSettings.Create(_actorSystem).WithRole("userSession")
                    );
                userInstance = _actorSystem.ActorOf(userSingletionProps, "userSingletonManager");

            }

            var userproxyProps = ClusterSingletonProxy.Props(
                    singletonManagerPath: "user/userSingletonManager",
                    settings: ClusterSingletonProxySettings.Create(_actorSystem).WithRole("userSession"));
            var UserSupervisorRef = _actorSystem.ActorOf(userproxyProps, "userSupervisorProxy");

            var headSupervisorProps = Props.Create(() => new HeadSupervisor(UserSupervisorRef, lobbySupervisorRef));
            _actorRef = _actorSystem.ActorOf(headSupervisorProps, "router");

            //var lobbySupervisorProps = Props.Create(() => new LobbySupervisor(signalRActorRef));
            //var newLobbySupervisor = _actorSystem.ActorOf(lobbySupervisorProps);

            //// Create the UserSupervisor actor with a reference to the SignalR actor
            //var userSupervisorProps = Props.Create(() => new UserSupervisor(signalRActorRef, newLobbySupervisor));
            //var newUserSupervisor = _actorSystem.ActorOf(userSupervisorProps);

            // Create the HeadSupervisor actor with a reference to the SignalR actor


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
