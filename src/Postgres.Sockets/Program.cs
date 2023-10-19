using System.Reflection;
using Postgres.Sockets.Core;
using Postgres.Sockets.Database;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(o =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services
    .RegisterPostgresDatabaseServices(builder.Configuration)
    .RegisterCoreServices();

builder.Services.AddMediator(o => o.ServiceLifetime = ServiceLifetime.Scoped);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(1),
    AllowedOrigins =
    {
        "http://localhost",
        "http://localhost:5077", 
        "https://localhost:7143"
    }
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Services.MigrateDatabase(builder.Configuration);

app.Run();

#pragma warning disable CA1050
namespace Postgres.Sockets
{
    public partial class Program { }
} //WebApplicationFactory support
#pragma warning restore CA1050
