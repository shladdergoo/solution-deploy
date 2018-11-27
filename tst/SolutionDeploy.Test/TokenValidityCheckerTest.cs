namespace SolutionDeploy.Test
{
    using System;

    using Xunit;

    using SolutionDeploy.Core;
    using SolutionDeploy.Vsts;

    public class TokenValidityCheckerTest
    {
        [Theory]
        [InlineData(1, 1000, 0, false)]
        [InlineData(500, 1000, 50, false)]
        [InlineData(1000, 1000, 0, true)]
        [InlineData(1050, 1000, 60, true)]
        [InlineData(0, 10, 60, false)]
        [InlineData(9, 10, 60, false)]
        [InlineData(10, 10, 60, true)]
        [InlineData(11, 10, 60, true)]
        public void IsExpired_ReturnsCorrectly(
            int acquiredAgo, int expiresIn, int earlyExpiry, bool expectedResult)
        {
            OAuthAccessTokens tokens = new OAuthAccessTokens
            {
                Acquired = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(acquiredAgo)),
                ExpiresIn = expiresIn
            };

            Assert.Equal(expectedResult, TokenValidityChecker.IsExpired(tokens, earlyExpiry));
        }

        [Fact]
        public void IsExpired_NullTokens_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TokenValidityChecker.IsExpired(null, 999));
        }

        [Fact]
        public void IsExpired_EarlyExpiryLessThanZero_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                TokenValidityChecker.IsExpired(new OAuthAccessTokens(), -1));
        }
    }
}
