using Akka.Actor;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Utils;
using ProtoChat.Actors;
using ProtoChat.Proto;

namespace ProtoChat.Services;

public sealed class KeyExchangeServiceImpl : KeyExchangeService.KeyExchangeServiceBase
{
    private readonly IActorRef _sessionManagerActor;

    public KeyExchangeServiceImpl(IActorRef sessionManagerActor)
    {
        _sessionManagerActor = sessionManagerActor;
    }

    public override async Task<KeyExchangeResponse> KeyExchange(KeyExchangeRequest request, ServerCallContext context)
    {
        byte[] key = GenerateKey();
        byte[] nonce = GenerateNonce();
        byte[] tag = GenerateTag();

        string clientId = Guid.NewGuid().ToString();

        var reply = await _sessionManagerActor.Ask<SessionActor.ISessionCreatedResult>(
            new SessionActor.SessionTrackRequest(clientId, key));

        if (reply is SessionActor.SessionRegistered)
        {
            return new KeyExchangeResponse
            {
                ClientId = clientId,
                Key = ByteString.CopyFrom(key),
                Nonce = ByteString.CopyFrom(nonce),
                Tag = ByteString.CopyFrom(tag)
            };
        }

        return new KeyExchangeResponse
        {
            Error = "Session already exists"
        };
    }

    private byte[] GenerateKey() => new byte[32];
    private byte[] GenerateNonce() => new byte[12];
    private byte[] GenerateTag() => new byte[16];
}
