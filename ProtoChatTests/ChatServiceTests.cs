using System.Security.Cryptography;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using FluentAssertions;
using Grpc.Core;
using Moq;
using ProtoChat.Actors;
using ProtoChat.Proto;
using ProtoChat.Services;
using Xunit;

namespace ProtoChatTests;

public class ChatServiceTests : TestKit
{
    [Fact]
    public async Task Chat_ShouldProcessMessages_WhenSessionIsInitialized()
    {
        // Arrange
        var sessionManager = Sys.ActorOf(Props.Create(() => new SessionManagerActor()));
        var service = new ChatServiceImpl(sessionManager);

        var mockRequestStream = new Mock<IAsyncStreamReader<EncryptedMessage>>();
        var mockResponseStream = new Mock<IServerStreamWriter<EncryptedMessage>>();
        var mockContext = new Mock<ServerCallContext>();

        string clientId = "test-client-id";
        string message = "test-message";
        byte[] rootKey = RandomNumberGenerator.GetBytes(32);
        byte[] nonce = "123456789qwe"u8.ToArray();
        byte[] tag = "123456789qwertyu"u8.ToArray();
        
        var aesGcm = new AesGcm(rootKey);
        byte[] cipherText = new byte[message.Length];
        
        aesGcm.Encrypt(nonce, System.Text.Encoding.UTF8.GetBytes(message), cipherText, tag);
        
        var encryptedMessage = new EncryptedMessage
        {
            Sender = clientId,
            CipherPayload = Google.Protobuf.ByteString.CopyFrom(cipherText),
            Nonce = Google.Protobuf.ByteString.CopyFrom(nonce),
            Tag = Google.Protobuf.ByteString.CopyFrom(tag)
        };

        mockRequestStream.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockRequestStream.Setup(x => x.Current).Returns(encryptedMessage);

       
        // Ensure session is initialized
        sessionManager.Tell(new SessionActor.SessionTrackRequest(clientId, rootKey));
        ExpectMsg<SessionActor.SessionRegistered>();

        sessionManager.Tell(new SessionActor.IsInitialized(clientId));
        ExpectMsg<SessionActor.SessionIsNotInitialized>();

        sessionManager.Tell(new SessionActor.InitializeStreamWriter(clientId, mockResponseStream.Object));
        sessionManager.Tell(new SessionActor.IsInitialized(clientId));
        ExpectMsg<SessionActor.SessionIsInitialized>();
        
        // Act
        await service.Chat(mockRequestStream.Object, mockResponseStream.Object, mockContext.Object);

        service.ProcessedMessagesCount.Should().Be(1);
    }
}