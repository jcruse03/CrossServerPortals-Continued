using Lunarbin.Valheim.CrossServerPortals;
using Xunit;

namespace CrossServerPortals.Tests;

public class TeleportInfoTests
{
    [Theory]
    [InlineData("home|example.com:2459|hub", "example.com", 2459, "hub")]
    [InlineData("home|192.0.2.10", "192.0.2.10", 2456, "")]
    [InlineData("home|server-name.example:65535|target-with-dash", "server-name.example", 65535, "target-with-dash")]
    [InlineData("home|[2001:db8::10]:2462|hub", "2001:db8::10", 2462, "hub")]
    [InlineData("home|2001:db8::10", "2001:db8::10", 2456, "")]
    public void ParsesServerDestinations(string tag, string host, ushort port, string target)
    {
        TeleportInfo info = Assert.IsType<TeleportInfo>(TeleportInfo.ParsePortalTag(tag));
        Assert.Equal(TeleportInfo.PortalType.Server, info.Type);
        Assert.Equal(host, info.Address);
        Assert.Equal(port, info.Port);
        Assert.Equal(target, info.TargetTag);
    }

    [Fact]
    public void ParsesWorldNamesWithSpaces()
    {
        TeleportInfo info = Assert.IsType<TeleportInfo>(TeleportInfo.ParsePortalTag("dock|world:My Large World|arrival"));
        Assert.Equal(TeleportInfo.PortalType.World, info.Type);
        Assert.Equal("My Large World", info.Address);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ordinary portal")]
    [InlineData("|example.com:2456")]
    [InlineData("home|world:")]
    [InlineData("home|bad host:2456")]
    [InlineData("home|example.com:0")]
    [InlineData("home|example.com:65536")]
    [InlineData("home|example.com:notaport")]
    [InlineData("home|example.com|target|extra")]
    public void RejectsInvalidTags(string? tag)
    {
        Assert.Null(TeleportInfo.ParsePortalTag(tag!));
    }
}
