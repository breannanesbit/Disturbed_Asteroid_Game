using RaftElection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var nodes = Environment.GetEnvironmentVariable("NODES")?.Split(',')?.ToList() ?? [];
Console.WriteLine(nodes[1]);

var logFactory = new LoggerFactory();
var logger = logFactory.CreateLogger<Election>();

var election = new Election(nodes, logger);
builder.Services.AddSingleton<Election>(election);


//builder.Services.AddSingleton<Election>(serviceProvider =>
//{
//    var logger = serviceProvider.GetRequiredService<ILogger<Election>>();
//    return new Election(nodes, logger);

//});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthorization();

app.MapControllers();

app.Run();
