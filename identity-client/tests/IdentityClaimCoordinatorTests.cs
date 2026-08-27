using Driftwood.Identity;
using Xunit;

public sealed class IdentityClaimCoordinatorTests
{
    [Fact]
    public void AuthenticatedClaimCompletesBeforeSpawnMayProceed()
    {
        FakeSender sender = new FakeSender();
        IdentityClaimCoordinator coordinator = new IdentityClaimCoordinator(sender);

        bool ok = coordinator.ClaimBeforeSpawn(
            "203.0.113.10", 22003, 2, 76561198045115545UL, "dayaffe99",
            out string? failure);

        Assert.True(ok);
        Assert.Null(failure);
        Assert.True(sender.CompletedSynchronously);
        Assert.NotNull(sender.LastRequest);
        Assert.Equal("2", sender.LastRequest!.ClientId);
    }

    [Fact]
    public void AFailedClaimBlocksSpawnInsteadOfSilentlyCreatingAnotherCharacter()
    {
        FakeSender sender = new FakeSender { Failure = "timed out" };
        IdentityClaimCoordinator coordinator = new IdentityClaimCoordinator(sender);

        bool ok = coordinator.ClaimBeforeSpawn(
            "203.0.113.10", 22003, 2, 76561198045115545UL, "dayaffe99",
            out string? failure);

        Assert.False(ok);
        Assert.Equal("timed out", failure);
    }

    private sealed class FakeSender : IIdentityClaimSender
    {
        public string? Failure { get; set; }
        public bool CompletedSynchronously { get; private set; }
        public IdentityClaimRequest? LastRequest { get; private set; }

        public bool TrySend(IdentityClaimRequest request, out string? failure)
        {
            LastRequest = request;
            CompletedSynchronously = true;
            failure = Failure;
            return failure is null;
        }
    }
}
