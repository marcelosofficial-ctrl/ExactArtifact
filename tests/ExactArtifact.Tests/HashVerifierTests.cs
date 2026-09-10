using ExactArtifact.Core;

namespace ExactArtifact.Tests;

public sealed class HashVerifierTests
{
    [Theory]
    [InlineData("abcdef", "abcdef")]
    [InlineData("ABCDEF", "abcdef")]
    [InlineData("ab-cd-ef", "abcdef")]
    [InlineData("ab cd ef", "abcdef")]
    [InlineData("  abcdef  ", "abcdef")]
    public void IsMatch_EquivalentHashes_ReturnsTrue(
        string calculated,
        string expected)
    {
        Assert.True(HashVerifier.IsMatch(calculated, expected));
    }

    [Fact]
    public void IsMatch_DifferentHashes_ReturnsFalse()
    {
        Assert.False(HashVerifier.IsMatch("aaaaaaaa", "bbbbbbbb"));
    }
}