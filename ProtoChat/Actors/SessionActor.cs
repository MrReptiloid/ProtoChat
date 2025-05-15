using Akka.Actor;
using Grpc.Core;
using ProtoChat.Proto;
using Status = Grpc.Core.Status;

namespace ProtoChat.Actors;

public class SessionActor : ReceiveActor
{
    private readonly string _clientId;
    private IActorRef _streamWriter;
    private IActorRef _aesActor;
    private byte[]? _rootKey;

    public SessionActor(string clientId, byte[] rootKey)
    {
        _clientId = clientId;
        _rootKey = rootKey;
     
        _aesActor = Context.ActorOf(AesGcmActor.Props());
        
        Become(WaitingForCreated);
    }

    private void WaitingForCreated()
    {
        Receive<SessionTrackRequest>(_ =>
        {
            Sender.Tell(SessionRegistered.Instance);
            Become(WaitingForStreamWriter);
        });
        
        ReceiveAny(_ => Sender.Tell(new InvalidOperationException("Session not created")));
    }

    private void WaitingForStreamWriter()
    {
        Receive<InitializeStreamWriter>(msg =>
        {
            _streamWriter = Context.ActorOf(StreamWriterActor.Props(msg.StreamWriter));
            Become(ActiveSession);
        });
        
        Receive<IsInitialized>(_ =>
        {
            Sender.Tell(SessionIsNotInitialized.Instance);
        });
        
        ReceiveAny(_ => Sender.Tell(new InvalidOperationException("Stream writer not initialized")));
    }
    
    private void ActiveSession()
    {
        Receive<IsInitialized>(_ =>
        {
            Sender.Tell(!Equals(ActorRefs.Nobody, _streamWriter)
                ? SessionIsInitialized.Instance 
                : SessionIsNotInitialized.Instance);
        });
        
        Receive<PlainTextMessage>(msg =>
        {
            _aesActor.Tell(new AesGcmActor.EncryptRequest(msg.Message));
        });

        Receive<AesGcmActor.EncryptResponse>(encrypted =>
        {
            EncryptedMessage response = new()
            {
                Sender = _clientId,
                CipherPayload = Google.Protobuf.ByteString.CopyFrom(encrypted.CipherText),
                Nonce = Google.Protobuf.ByteString.CopyFrom(encrypted.Nonce),
                Tag = Google.Protobuf.ByteString.CopyFrom(encrypted.Tag),
            };
            
            _streamWriter.Tell(response);
        });

        Receive<IncomingEncryptedMessage>(msg =>
        {
            _aesActor.Tell(new AesGcmActor.DecryptRequest(
                msg.CipherText,
                msg.Nonce,
                msg.Tag,
                _rootKey ?? throw new InvalidOperationException("Key not generated yet")));
        });

        Receive<AesGcmActor.DecryptResponse>(decrypted =>
        {
            Console.WriteLine(decrypted.Message);
        });
    }
    
    public static Props Props(string clientId, byte[] rootKey) =>
        Akka.Actor.Props.Create(() => new SessionActor(clientId, rootKey));

    public interface ISessionCreatedResult { }
    public sealed class SessionRegistered : ISessionCreatedResult
    {
        public static SessionRegistered Instance { get; } = new();
        private SessionRegistered() { }
    }
    public sealed class SessionAlreadyExist : ISessionCreatedResult
    {
        public static SessionAlreadyExist Instance { get; } = new();
        private SessionAlreadyExist() { }
    }
    
    public sealed record InitializeStreamWriter(string ClientId, IServerStreamWriter<EncryptedMessage> StreamWriter);
    public sealed record IsInitialized (string ClientId);
    
    public record SessionTrackRequest(string ClientId, byte[] RootKey);

    public interface ISessionInitializedState { }

    public sealed class SessionIsInitialized : ISessionInitializedState
    {
        public static SessionIsInitialized Instance { get; } = new();
        private SessionIsInitialized() { }
    }
    public sealed class SessionIsNotInitialized : ISessionInitializedState
    {
        public static SessionIsNotInitialized Instance { get; } = new();
        private SessionIsNotInitialized() { }
    }
    
    public sealed record PlainTextMessage(string Message);
    public sealed record IncomingEncryptedMessage(string ClientId, byte[] CipherText, byte[] Nonce, byte[] Tag);
}