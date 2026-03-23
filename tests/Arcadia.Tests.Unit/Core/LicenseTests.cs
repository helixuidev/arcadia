using Arcadia.Core.Licensing;
using FluentAssertions;
using Xunit;

namespace Arcadia.Tests.Unit.Core;

public class LicenseTests : IDisposable
{
    public LicenseTests()
    {
        ArcadiaLicense.Reset();
    }

    public void Dispose()
    {
        ArcadiaLicense.Reset();
    }

    [Fact]
    public void NoKey_DefaultsToCommunity()
    {
        ArcadiaLicense.GetTier().Should().Be(LicenseTier.Community);
        ArcadiaLicense.IsCommunity.Should().BeTrue();
        ArcadiaLicense.IsProLicensed.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("INVALID")]
    [InlineData("ARC-1234")]
    [InlineData("ARC-XXXX-YYYY")]
    [InlineData("ARC-!!!!-@@@@-####")]
    [InlineData("XYZ-P1A2-B3C4-M4TC")]
    public void InvalidFormat_DefaultsToCommunity(string key)
    {
        ArcadiaLicense.SetKey(key);

        ArcadiaLicense.GetTier().Should().Be(LicenseTier.Community);
        ArcadiaLicense.IsCommunity.Should().BeTrue();
        ArcadiaLicense.IsProLicensed.Should().BeFalse();
    }

    [Fact]
    public void InvalidChecksum_DefaultsToCommunity()
    {
        // Valid format but wrong checksum group
        ArcadiaLicense.SetKey("ARC-P1A2-B3C4-ZZZZ");

        ArcadiaLicense.GetTier().Should().Be(LicenseTier.Community);
        ArcadiaLicense.IsCommunity.Should().BeTrue();
        ArcadiaLicense.IsProLicensed.Should().BeFalse();
    }

    [Fact]
    public void ValidProKey_ActivatesProTier()
    {
        ArcadiaLicense.SetKey("ARC-P1A2-B3C4-M4TC");

        ArcadiaLicense.GetTier().Should().Be(LicenseTier.Pro);
        ArcadiaLicense.IsProLicensed.Should().BeTrue();
        ArcadiaLicense.IsCommunity.Should().BeFalse();
    }

    [Fact]
    public void ValidEnterpriseKey_ActivatesEnterpriseTier()
    {
        ArcadiaLicense.SetKey("ARC-E5F6-G7H8-ICP5");

        ArcadiaLicense.GetTier().Should().Be(LicenseTier.Enterprise);
        ArcadiaLicense.IsProLicensed.Should().BeTrue();
        ArcadiaLicense.IsCommunity.Should().BeFalse();
    }

    [Fact]
    public void ProKey_TierDetection()
    {
        ArcadiaLicense.SetKey("ARC-P1A2-B3C4-M4TC");

        ArcadiaLicense.GetTier().Should().Be(LicenseTier.Pro);
    }

    [Fact]
    public void EnterpriseKey_TierDetection()
    {
        ArcadiaLicense.SetKey("ARC-E5F6-G7H8-ICP5");

        ArcadiaLicense.GetTier().Should().Be(LicenseTier.Enterprise);
    }

    [Fact]
    public void SetKey_Null_RevertsToCommunity()
    {
        ArcadiaLicense.SetKey("ARC-P1A2-B3C4-M4TC");
        ArcadiaLicense.IsProLicensed.Should().BeTrue();

        ArcadiaLicense.SetKey(null);

        ArcadiaLicense.GetTier().Should().Be(LicenseTier.Community);
        ArcadiaLicense.IsCommunity.Should().BeTrue();
    }

    [Fact]
    public void SetKey_CanBeCalledMultipleTimes()
    {
        ArcadiaLicense.SetKey("ARC-P1A2-B3C4-M4TC");
        ArcadiaLicense.GetTier().Should().Be(LicenseTier.Pro);

        ArcadiaLicense.SetKey("ARC-E5F6-G7H8-ICP5");
        ArcadiaLicense.GetTier().Should().Be(LicenseTier.Enterprise);
    }

    [Fact]
    public void GeneratedProKey_Validates()
    {
        ArcadiaLicense.SetKey("ARC-PWNX-BZTC-2LPX");
        ArcadiaLicense.IsProLicensed.Should().BeTrue();
        ArcadiaLicense.GetTier().Should().Be(LicenseTier.Pro);
    }

    [Fact]
    public void GeneratedEnterpriseKey_Validates()
    {
        ArcadiaLicense.SetKey("ARC-EM7T-W97C-LG9R");
        ArcadiaLicense.IsProLicensed.Should().BeTrue();
        ArcadiaLicense.GetTier().Should().Be(LicenseTier.Enterprise);
    }
}
