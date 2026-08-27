using System.Text.Json;
using Driftwood.Identity;
using Xunit;

public sealed class IdentityClaimRequestTests
{
    [Fact]
    public void BuildsTheServerContractWithDecimalStrings()
    {
        IdentityClaimRequest request = IdentityClaimRequest.Create(
            "203.0.113.10",
            22003,
            2,
            76561198045115545UL,
            "dayaffe99");

        Assert.Equal("http://203.0.113.10:22004/api/v1/identity", request.Endpoint.ToString());
        using JsonDocument json = JsonDocument.Parse(request.Body);
        Assert.Equal(JsonValueKind.String, json.RootElement.GetProperty("clientId").ValueKind);
        Assert.Equal("2", json.RootElement.GetProperty("clientId").GetString());
        Assert.Equal(JsonValueKind.String, json.RootElement.GetProperty("steamId").ValueKind);
        Assert.Equal("76561198045115545", json.RootElement.GetProperty("steamId").GetString());
        Assert.Equal("dayaffe99", json.RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public void EscapesPersonaNamesAsJson()
    {
        IdentityClaimRequest request = IdentityClaimRequest.Create(
            "example.test", 22003, 1, 76561198045115545UL, "fish \"crew\"\\one");

        using JsonDocument json = JsonDocument.Parse(request.Body);
        Assert.Equal("fish \"crew\"\\one", json.RootElement.GetProperty("name").GetString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65535)]
    public void RejectsPortsWithoutAValidAdjacentHttpPort(int gamePort)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => IdentityClaimRequest.Create(
            "example.test", gamePort, 1, 76561198045115545UL, "dayaffe99"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("host/path")]
    [InlineData("host\\path")]
    [InlineData("host\nheader")]
    public void RejectsInvalidHosts(string host)
    {
        Assert.Throws<ArgumentException>(() => IdentityClaimRequest.Create(
            host, 22003, 1, 76561198045115545UL, "dayaffe99"));
    }

    [Fact]
    public void RejectsAnUnassignedConnectionId()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => IdentityClaimRequest.Create(
            "example.test", 22003, -1, 76561198045115545UL, "dayaffe99"));
    }
}
