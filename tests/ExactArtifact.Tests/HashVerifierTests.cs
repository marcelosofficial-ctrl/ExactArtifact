using ExactArtifact.Core;

namespace ExactArtifact.Tests;

public sealed class HashVerifierTests
{
    private const string Hash =
        "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9";

    [Theory]
    [InlineData("b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9")]
    [InlineData("B94D27B9934D3E08A52E52D7DA7DABFAC484EFE37A5380EE9088F7ACE2EFCDE9")]
    [InlineData("b9 4d 27 b9 93 4d 3e 08 a5 2e 52 d7 da 7d ab fa c4 84 ef e3 7a 53 80 ee 90 88 f7 ac e2 ef cd e9")]
    [InlineData("b9-4d-27-b9-93-4d-3e-08-a5-2e-52-d7-da-7d-ab-fa-c4-84-ef-e3-7a-53-80-ee-90-88-f7-ac-e2-ef-cd-e9")]
    [InlineData("  b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9  ")]
    public void IsMatch_EquivalentHashes_ReturnsTrue(
        string expected)
    {
        Assert.True(
            HashVerifier.IsMatch(Hash, expected));
    }

    [Fact]
    public void IsMatch_DifferentHashes_ReturnsFalse()
    {
        Assert.False(
            HashVerifier.IsMatch(
                Hash,
                new string('0', 64)));
    }

    [Fact]
    public void NormalizeSha256_TooShort_Throws()
    {
        Assert.Throws<FormatException>(
            () => HashVerifier.NormalizeSha256("abcdef"));
    }

    [Fact]
    public void NormalizeSha256_NonHex_Throws()
    {
        Assert.Throws<FormatException>(
            () => HashVerifier.NormalizeSha256(
                new string('z', 64)));
    }
}