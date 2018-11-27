namespace SolutionDeploy.Test
{
    using SolutionDeploy.Core;
    using SolutionDeploy.Vsts;

    internal class TestableVstsOAuthAuthenticator_NotExpired: VstsOAuthAuthenticator
    {
        internal TestableVstsOAuthAuthenticator_NotExpired(IHttpClient httpClient, ITokenRepository tokenRepository, VstsConfig vstsConfig) : base(httpClient, tokenRepository, vstsConfig)
        {
        }

        protected override bool IsExpired(OAuthAccessTokens tokens)
        {
            return false;
        }
    }
}