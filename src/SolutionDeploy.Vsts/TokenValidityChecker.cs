namespace SolutionDeploy.Vsts
{
    using System;

    using SolutionDeploy.Core;

    public static class TokenValidityChecker
    {
        public static bool IsExpired(OAuthAccessTokens tokens, int earlyExpiry)
        {
            if (tokens == null) { throw new ArgumentNullException(nameof(tokens)); }
            if (earlyExpiry < 0) { throw new ArgumentException("argument cannot be less than zero", nameof(earlyExpiry)); }

            int effectiveExpiry;
            if (earlyExpiry > tokens.ExpiresIn)
            {
                effectiveExpiry = tokens.ExpiresIn;
            }
            else
            {
                effectiveExpiry = tokens.ExpiresIn - earlyExpiry;
            }

            return tokens.Acquired.AddSeconds(effectiveExpiry) < DateTime.UtcNow;
        }
    }
}
