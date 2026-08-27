using Driftwood.Identity;
using Xunit;

public sealed class ConnectTargetTests
{
    [Fact]
    public void ReadsTheTargetAfterDriftwoodConnectDisarmsTheConfig()
    {
        const string config = """
            [Client]
            Enable = false
            AutoConnect = false
            ConnectAddress = 203.0.113.10
            ConnectPort = 22003
            ArmedAtUtc =
            """;

        ConnectTarget target = ConnectTarget.Parse(config);

        Assert.Equal("203.0.113.10", target.Host);
        Assert.Equal(22003, target.GamePort);
    }
}
