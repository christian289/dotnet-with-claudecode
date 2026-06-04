using WpfDevPackMcp.Git;
using Xunit;

namespace WpfDevPackMcp.Tests;

public sealed class RepoRefresherTests
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(60);

    [Fact]
    public void ShouldPull_True_WhenNeverPulled()
        => Assert.True(RepoRefresher.ShouldPull(lastPull: null, now: DateTimeOffset.UtcNow, Ttl, force: false));

    [Fact]
    public void ShouldPull_True_WhenTtlElapsed()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.True(RepoRefresher.ShouldPull(now.AddMinutes(-61), now, Ttl, force: false));
    }

    [Fact]
    public void ShouldPull_False_WithinTtl()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.False(RepoRefresher.ShouldPull(now.AddMinutes(-10), now, Ttl, force: false));
    }

    [Fact]
    public void ShouldPull_True_WhenForced_EvenWithinTtl()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.True(RepoRefresher.ShouldPull(now.AddMinutes(-1), now, Ttl, force: true));
    }
}
