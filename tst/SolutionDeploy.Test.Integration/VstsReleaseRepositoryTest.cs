namespace SolutionDeploy.Test.Integration
{
    using System;
    using System.IO;

    using Xunit;

    using SolutionDeploy.Core;
    using SolutionDeploy.Vsts;

    public class VstsReleaseRepositoryTest
    {
        private const string TokenFilename = "solutiondeploy_tokens.json";
        string fullTokenFilename =
            Path.Combine(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"), TokenFilename);
        IReleaseRepository sut;

        [Fact]
        public void GetReleaseId__Succeeds()
        {
            VstsConfig vstsConfig = GetVstsConfig();
            IVstsReleaseClient releaseClient = new VstsSyncReleaseClient(vstsConfig);
            IHttpClient httpClient = new HttpClient();
            ITokenRepository tokenRepository =
                new JsonFileTokenRepository(new FileSystem());
            IAuthenticator authenticator =
                new VstsOAuthAuthenticator(httpClient, tokenRepository, vstsConfig);

            File.Copy($"..//..//..//testdata//{TokenFilename}", fullTokenFilename, true);

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            string result = this.sut.GetReleaseId("samplepackage", "1.1.11");

            File.Copy(fullTokenFilename, $"..//..//..//testdata//{TokenFilename}", true);

            Assert.NotNull(result);
        }

        private static VstsConfig GetVstsConfig()
        {
            return new VstsConfig
            {
                TokenUrl = @"https://app.vssps.visualstudio.com/oauth2/token",
                AuthorizationClientSecret = "",
                AuthorizationCallbackUrl = "",
                AuthorizationUserId = "SolutionDeployConsole",
                AuthorizedScopes = new string[] { "vso.build_execute", "vso.release_manage" },
                TokenEarlyExpiry = 10,
                BaseUrl = @"",
                ProjectName = "CI"
            };
        }
    }
}
