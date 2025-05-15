using Akka.Actor;
using Grpc.Core;
using ProtoChat.Proto;

namespace ProtoChat.Actors;

public class StreamWriterActor : ReceiveActor
{
    private readonly IServerStreamWriter<EncryptedMessage> _writer;

    public StreamWriterActor(IServerStreamWriter<EncryptedMessage> writer)
    {
        _writer = writer;
        
        ReceiveAsync<EncryptedMessage>(async msg =>
        {
            await _writer.WriteAsync(msg);
        });
    }
    
    public static Props Props(IServerStreamWriter<EncryptedMessage> writer) =>
        Akka.Actor.Props.Create(() => new StreamWriterActor(writer));
}