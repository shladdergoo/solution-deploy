namespace SolutionDeploy.Test.Integration
{
    using System;
    using System.IO;

    using NSubstitute;
    using Xunit;

    using SolutionDeploy.Core;
    using SolutionDeploy.Vsts;

    public class VstsOAuthAuthenticatorTest
    {
        private const string TokenFilename = "solutiondeploy_tokens.json";
        string fullTokenFilename =
            Path.Combine(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"), TokenFilename);

        private IAuthenticator sut;

        [Fact]
        public void Authenticate_RefreshTokens_Succeeds()
        {
            IHttpClient httpClient = new HttpClient();
            ITokenRepository tokenRepository = new JsonFileTokenRepository(new FileSystem());
            VstsConfig vstsConfig = GetVstsConfig();

            File.Copy($"..//..//..//testdata//{TokenFilename}", fullTokenFilename, true);

            this.sut = new VstsOAuthAuthenticator(httpClient, tokenRepository, vstsConfig);

            AuthenticationResult result = this.sut.Authenticate();

            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);

            File.Copy(fullTokenFilename, $"..//..//..//testdata//{TokenFilename}", true);
        }

        private static VstsConfig GetVstsConfig()
        {
            return new VstsConfig
            {
                BaseUrl = "",
                ProjectName = "CI",
                TokenUrl = "https://app.vssps.visualstudio.com/oauth2/token",
                AuthorizationClientId = "",
                AuthorizationClientSecret = "",
                AuthorizationCallbackUrl = "",
                TokenEarlyExpiry = 3595,
                AuthorizationUserId = "SolutionDeployConsole",
                AuthorizedScopes = new string[]
                {
                    "vso.build_execute",
                    "vso.release_manage"
                }
            };
        }
    }
}
