using Akka.Actor;
using Akka.AspNetCore;
using Akka.Configuration;
using blazorserverapptest.Components;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Grafana.Loki;
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
    .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        //.AddConsoleExporter()
        .AddOtlpExporter());

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.WriteTo.Console()
    .Enrich.WithProperty("job", "your-api-job")
    .Enrich.WithExceptionDetails()
    .WriteTo.GrafanaLoki("http://loki:3100");


    /*.WriteTo.Sink(new GrafanaLokiSink("http://loki:3100"));*/
});


builder.Services.AddSingleton(provider =>
{
    var config = ConfigurationFactory.ParseString(File.ReadAllText("akka.conf"));
    return ActorSystem.Create("YourActorSystem", config);
});

builder.Services.AddSingleton<IActorBridge, AkkaService>();

// starts the IHostedService, which creates the ActorSystem and actors
builder.Services.AddHostedService<AkkaService>(sp => (AkkaService)sp.GetRequiredService<IActorBridge>());
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
