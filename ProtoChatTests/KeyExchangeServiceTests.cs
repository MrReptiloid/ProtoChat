using Akka.Actor;
using Akka.TestKit.Xunit2;
using FluentAssertions;
using ProtoChat.Actors;
using ProtoChat.Proto;
using ProtoChat.Services;
using ProtoChatTests.Utilities;
using Xunit;

namespace ProtoChatTests;

public class KeyExchangeServiceTests : TestKit
{
    [Fact]
    public async Task KeyExchange_ShouldReturnSuccess_WhenSessionIsRegistered()
    {
        // Arrange
        var sessionManager = Sys.ActorOf(Props.Create(() => new SessionManagerActor()));
        var service = new KeyExchangeServiceImpl(sessionManager);

        var request = new KeyExchangeRequest();
        var context = TestServerCallContext.Create();

        // Act
        var response = await service.KeyExchange(request, context);
        
        // Assert
        response.ClientId.Should().NotBeNullOrEmpty();
        response.Key.Length.Should().Be(32);
        response.Nonce.Length.Should().Be(12);
        response.Tag.Length.Should().Be(16);
        response.Error.Should().BeNullOrEmpty();
    }
}