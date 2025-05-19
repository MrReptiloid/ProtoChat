using Akka.Actor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ProtoChat.Actors;
using ProtoChat.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps(); // Use default development certificate
    });
});

// Create ActorSystem
ActorSystem actorSystem = ActorSystem.Create("ProtoChatSystem");

// Create SessionManagerActor
IActorRef sessionManagerActor = actorSystem.ActorOf(Props.Create(() => new SessionManagerActor()), "sessionManager");

// Register ActorSystem and SessionManagerActor
builder.Services.AddSingleton(actorSystem);
builder.Services.AddSingleton(sessionManagerActor);

// Add gRPC services
builder.Services.AddGrpc();
builder.Services.AddSingleton<KeyExchangeServiceImpl>(provider =>
    new KeyExchangeServiceImpl(provider.GetRequiredService<IActorRef>()));

var app = builder.Build();

// Map gRPC services
app.MapGrpcService<ChatServiceImpl>();
app.MapGrpcService<KeyExchangeServiceImpl>();

// Run the application
app.Run();