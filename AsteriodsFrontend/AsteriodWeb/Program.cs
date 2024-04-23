using AsteriodWeb.Components;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Sinks.Loki;
using Shared;
using Shared.Metrics;
using Shared.SignalRService;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();


builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(
        serviceNamespace: "demo-namespace",
        serviceName: builder.Environment.ApplicationName,
        serviceVersion: Assembly.GetEntryAssembly()?.GetName().Version?.ToString(),
        serviceInstanceId: Environment.MachineName
    ).AddAttributes(new Dictionary<string, object>
    {
        { "deployment.environment", builder.Environment.EnvironmentName }
    }))
    .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation()
        //.AddConsoleExporter()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddMeter(DefineMetrics.lobbyMeter.Name)
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        //.AddConsoleExporter()
        .AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri("http://otel-collector:4317/");
        })
        .AddPrometheusExporter()
        );

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.Enrich.FromLogContext()
        .Enrich.WithProperty("job", "your-api-job")
        .Enrich.WithExceptionDetails()
        .WriteTo.Console()
        .WriteTo.LokiHttp("http://loki:3100")
        .WriteTo.GrafanaLoki("http://loki:3100");
});


// builder.Services.AddSingleton(provider =>
// {
//     var config = ConfigurationFactory.ParseString(File.ReadAllText("akka.conf"));
//     return ActorSystem.Create("YourActorSystem", config);
// });
builder.Services.AddSingleton<IActorBridge, AkkaService>();
builder.Services.AddSingleton<ActorSignalRService>();

// // starts the IHostedService, which creates the ActorSystem and actors
builder.Services.AddHostedService<AkkaService>(sp => (AkkaService)sp.GetRequiredService<IActorBridge>());


var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapPrometheusScrapingEndpoint();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
