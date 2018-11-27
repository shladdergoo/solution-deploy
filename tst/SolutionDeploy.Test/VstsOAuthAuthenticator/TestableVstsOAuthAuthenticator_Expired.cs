namespace SolutionDeploy.Test
{
    using System.Net;

    using SolutionDeploy.Core;
    using SolutionDeploy.Vsts;

    internal class TestableVstsOAuthAuthenticator_Expired: VstsOAuthAuthenticator
    {
        internal TestableVstsOAuthAuthenticator_Expired(IHttpClient httpClient, ITokenRepository tokenRepository, VstsConfig vstsConfig) : base(httpClient, tokenRepository, vstsConfig)
        {
        }

        protected override bool IsExpired(OAuthAccessTokens tokens)
        {
            return true;
        }

        protected override OAuthAccessTokens GetTokensFromResponse(HttpWebResponse response)
        {
            return new OAuthAccessTokens
            {
                AccessToken = "foo",
                RefreshToken = "bar"
            };
        }
    }
}
