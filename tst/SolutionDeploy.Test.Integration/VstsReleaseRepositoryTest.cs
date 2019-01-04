namespace SolutionDeploy.Test.Integration
{
    using System;
    using System.IO;

    using Xunit;

    using SolutionDeploy.Core;
    using SolutionDeploy.Vsts;

    public class VstsReleaseRepositoryTest
    {
        private const string TokenFilename = "solutiondeploy_solutiondeploy_tokens.json";
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
                AuthorizationClientSecret = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJjaWQiOiIwN2ZiNGFkYi0yMTZjLTQ4ZDEtYmQ5MC1lMjE4MTI5Y2ViNGQiLCJjc2kiOiI3NzRmNzAxOC03YWRkLTRjYzktOGJjMy03YTczNWU0NWVjNTciLCJuYW1laWQiOiIxZWFmZTE5ZS01ZGQ3LTQ4MmMtODJmYi03OGUyMzE0ZTYxNDciLCJpc3MiOiJhcHAudnNzcHMudmlzdWFsc3R1ZGlvLmNvbSIsImF1ZCI6ImFwcC52c3Nwcy52aXN1YWxzdHVkaW8uY29tIiwibmJmIjoxNTI4OTk2NzU1LCJleHAiOjE2ODY3NjMxNTV9.wvvoxDeBSKH4QzXUyWUAfmDmLwC3Si1GTeI_4XSd5tZhwEU3dYNiEFIvAfCAWrt81NPMAXGkpNrH7oogkyoW8NtnzhBiAo8z0PE2Aw8cVQh95JlNH1XNhd3G7yv0wpzETVP6xfHRpJo1bYPVJAHg2d9PSEty_jMN8CdWUxf3R6VUxw-CCQkiV8STCTc-GKtRFoESV_gwg2KryEExRr3BCvq8vAKV8ThD-aES250oqyJ8twzIzkatE7INvmuxJeIY81X3twV6q4EpfEaiaQ7KHF7gpO_5zf5coAXX0L829Hm5qlpnME8548Q5aWTtkoC6fGg2ldKxGhIlM3IPhYTNqA",
                AuthorizationCallbackUrl = "https://hcc-devops-authorization.azurewebsites.net/api/auth",
                AuthorizationUserId = "SolutionDeployConsole",
                AuthorizedScopes = new string[] { "vso.build_execute", "vso.release_manage" },
                TokenEarlyExpiry = 10,
                BaseUrl = @"https://hcc-devops.visualstudio.com",
                ProjectName = "CI"
            };
        }
    }
}
