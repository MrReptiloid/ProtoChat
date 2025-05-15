using Akka.Actor;
using Akka.Event;

namespace ProtoChat.Actors;

public class SessionManagerActor : ReceiveActor
{
    private readonly Dictionary<string, IActorRef> clientIdToActor = new();
    private readonly Dictionary<IActorRef, string> actorToClientId = new();

    protected override void PreStart() => Log.Info($"Session manager started");
    protected override void PostStop() => Log.Info($"Session manager stopped");

    protected ILoggingAdapter Log { get; } = Context.GetLogger();


    public SessionManagerActor()
    {
        Receive<SessionActor.SessionTrackRequest>(msg => HandleSessionTrackRequest(msg));
        Receive<SessionActor.InitializeStreamWriter>(msg => HandleInitializeStreamWriter(msg));
        Receive<SessionActor.IsInitialized>(msg => HandleIsInitialized(msg));
        Receive<SessionActor.IncomingEncryptedMessage>(msg => HandleIncomingEncryptedMessage(msg));
    }

    private void HandleSessionTrackRequest(SessionActor.SessionTrackRequest msg)
    {
        if (clientIdToActor.TryGetValue(msg.ClientId, out IActorRef? _))
        {
            Log.Info("Session already exists for client {msg.ClientId}");
            Sender.Tell(SessionActor.SessionAlreadyExist.Instance);
        }
        else
        {
            Log.Info("Creating session actor for client {msg.ClientId}");
            IActorRef sessionActor = Context.ActorOf(SessionActor.Props(msg.ClientId, msg.RootKey));
            actorToClientId.Add(sessionActor, msg.ClientId);
            clientIdToActor.Add(msg.ClientId, sessionActor);
            sessionActor.Forward(msg);
        }
    }
    private void HandleInitializeStreamWriter(SessionActor.InitializeStreamWriter msg)
    {
        if (clientIdToActor.TryGetValue(msg.ClientId, out IActorRef? actorRef))
        {
            actorRef.Forward(msg);
        }
        else
        {
            Log.Warning("No session found for client {msg.ClientId}");
        }
    }
    private void HandleIsInitialized(SessionActor.IsInitialized msg)
    {
        if (clientIdToActor.TryGetValue(msg.ClientId, out IActorRef? actorRef))
        {
            actorRef.Forward(msg);
        }
    }
    private void HandleIncomingEncryptedMessage(SessionActor.IncomingEncryptedMessage msg)
    {
        if (clientIdToActor.TryGetValue(msg.ClientId, out IActorRef? actorRef))
        {
            actorRef.Forward(msg);
        }
        else
        {
            Log.Warning("No session found for client {msg.ClientId}");
        }
    }
    
    public static Props Props() =>
        Akka.Actor.Props.Create(() => new SessionManagerActor());
}