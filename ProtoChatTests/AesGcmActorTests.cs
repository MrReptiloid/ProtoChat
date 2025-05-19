using Akka.TestKit.Xunit2;
using FluentAssertions;
using ProtoChat.Actors;
using Xunit;

namespace ProtoChatTests;

public class AesGcmActorTests : TestKit
{
    [Fact]
    public void AesGcmActor_Should_Encrypt_And_Decrypt_Message()
    {
        var probe = CreateTestProbe();
        var aesGcmActor = Sys.ActorOf(AesGcmActor.Props());
        
        byte[] originalMessage = "Hello, World!"u8.ToArray();
        
        aesGcmActor.Tell(new AesGcmActor.EncryptRequest(originalMessage), probe.Ref);
        var encryptResponse = probe.ExpectMsg<AesGcmActor.EncryptResponse>();
        
        encryptResponse.CipherPayload.Should().NotBeNullOrEmpty();
        encryptResponse.Key.Should().HaveCount(32);
        encryptResponse.Nonce.Should().HaveCount(12);
        encryptResponse.Tag.Should().HaveCount(16);

        aesGcmActor.Tell(new AesGcmActor.DecryptRequest(
            encryptResponse.CipherPayload,
            encryptResponse.Nonce,
            encryptResponse.Tag,
            encryptResponse.Key
        ), probe);
        
        var decryptResponse = probe.ExpectMsg<AesGcmActor.DecryptResponse>();

        decryptResponse.Payload.Should().BeEquivalentTo(originalMessage);
    }
}