using Akka.Actor;
using Akka.DependencyInjection;
using Actors.Classes;

namespace AsteriodsFrontend.Services;

public class AkkaService : IHostedService, IActorBridge
{
    private ActorSystem _actorSysetem;
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
        _actorSysetem = ActorSystem.Create("akka-universe", actorSystemSetup);
        _actorRef = _actorSysetem.ActorOf(Worker.Props(), "heavy-weight-word");
        _actorSysetem.WhenTerminated.ContinueWith(_ =>
        {
            _applicationLifetime.StopApplication();
        });
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await CoordinatedShutdown.Get(_actorSysetem).Run(CoordinatedShutdown.ClrExitReason.Instance);
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
