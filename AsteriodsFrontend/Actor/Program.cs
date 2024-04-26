using Shared;
using Shared.SignalRService;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Sinks.Loki;
using Shared.Metrics;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<IActorBridge, AkkaService>();
builder.Services.AddSingleton<ActorSignalRService>();

// // starts the IHostedService, which creates the ActorSystem and actors
builder.Services.AddHostedService<AkkaService>(sp => (AkkaService)sp.GetRequiredService<IActorBridge>());

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
        .AddMeter(DefineMetrics.lobbyMeterName)
        .AddAspNetCoreInstrumentation()
        // .AddRuntimeInstrumentation()
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapPrometheusScrapingEndpoint();
app.MapControllers();

app.Run();