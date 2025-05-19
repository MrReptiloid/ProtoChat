using Akka.Actor;
using ProtoChat.DataAccess;

namespace ProtoChat.Domain.Commands;

public class CommandContext
{
    public string SenderId { get; init; }
    public IActorRef SessionManger { get; init; }
    public IActorRef SelfActor { get; init; }

    public InMemoryUserStore UserStore { get; } = InMemoryUserStore.Instance;
}