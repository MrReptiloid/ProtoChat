using Grpc.Core;

namespace ProtoChatTests.Utilities;

public class TestServerCallContext : ServerCallContext
{
    public static ServerCallContext Create() => new TestServerCallContext();

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options) => null!;
    protected override string MethodCore => "TestMethod";
    protected override string HostCore => "localhost";
    protected override string PeerCore => "TestPeer";
    protected override DateTime DeadlineCore => DateTime.UtcNow.AddMinutes(1);
    protected override Metadata RequestHeadersCore => new Metadata();
    protected override CancellationToken CancellationTokenCore => CancellationToken.None;
    protected override Metadata ResponseTrailersCore => new Metadata();
    protected override Status StatusCore { get; set; }
    protected override WriteOptions WriteOptionsCore { get; set; }
    protected override AuthContext AuthContextCore => null!;
}