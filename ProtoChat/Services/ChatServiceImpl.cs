using Akka.Actor;
using Grpc.Core;
using Grpc.Core.Utils;
using ProtoChat.Actors;
using ProtoChat.Proto;

namespace ProtoChat.Services;

public sealed class ChatServiceImpl : ChatService.ChatServiceBase
{
    private readonly IActorRef _sessionManagerActor;
    public int ProcessedMessagesCount { get; private set; } = 0;

    public ChatServiceImpl(IActorRef sessionManagerActor)
    {
        _sessionManagerActor = sessionManagerActor;
    }

    public override async Task Chat(
        IAsyncStreamReader<EncryptedMessage> requestStream,
        IServerStreamWriter<EncryptedMessage> responseStream,
        ServerCallContext context)
    {
        string clientId = requestStream.Current.Sender;
        
        SessionActor.ISessionInitializedState? reply = 
            await _sessionManagerActor.Ask<SessionActor.ISessionInitializedState>(
                new SessionActor.IsInitialized(clientId));

        if (reply is SessionActor.SessionIsNotInitialized)
        {
            _sessionManagerActor.Tell(new SessionActor.InitializeStreamWriter(clientId, responseStream));
        }
        
        await requestStream.ForEachAsync(msg =>
        {
            _sessionManagerActor.Tell(new SessionActor.IncomingEncryptedMessage(
                clientId,
                msg.CipherPayload.ToByteArray(),
                msg.Nonce.ToByteArray(),
                msg.Tag.ToByteArray()));
            
            ProcessedMessagesCount++;
            
            return Task.CompletedTask;
        });
    }
}