namespace SolutionDeploy.Test.Integration
{
    using System;
    using System.IO;

    using Xunit;

    using SolutionDeploy.Core;
    using SolutionDeploy.Vsts;

    public class DeploymentServiceTest
    {
        private IDeploymentService sut;

        [Fact]
        public void Deploy_Latest_Succeeds()
        {
            File.Copy(@"../../../testdata/solutiondeploy_tokens.json", "solutiondeploy_tokens.json", true);
            File.Copy(@"../../../testdata/manifest.json", "manifest.json", true);

            sut = BuildService();

            sut.Deploy("authorizationservice", "Azure Prod");

            File.Copy("solutiondeploy_tokens.json", @"../../../testdata/solutiondeploy_tokens.json", true);
        }

        private static IDeploymentService BuildService()
        {
            VstsConfig vstsConfig = GetVstsConfig();
            IFileSystem fileSystem = new FileSystem();
            IHttpClient httpClient = new HttpClient();
            IProductManifestRepository manifestRepository = new JsonFileProductManifestRepository(fileSystem);
            ITokenRepository tokenRepository = new JsonFileTokenRepository(fileSystem);
            IVstsReleaseClient releaseClient = new VstsSyncReleaseClient(vstsConfig);
            IAuthenticator authenticator = new VstsOAuthAuthenticator(httpClient, tokenRepository, vstsConfig);
            IReleaseRepository releaseRepository = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);
            IServiceDeploymentExecutor deploymentExecutor = new VstsDeploymentExecutor(releaseRepository, vstsConfig);
            IServiceDeploymentHandler deploymentHandler = new SequentialDeploymentHandler(deploymentExecutor, 10);
            IDeploymentService service = new DeploymentService(manifestRepository, deploymentHandler);

            return service;
        }

        private static VstsConfig GetVstsConfig()
        {
            return new VstsConfig
            {
                BaseUrl = "https://hcc-devops.visualstudio.com",
                ProjectName = "CI",
                TokenUrl = "https://app.vssps.visualstudio.com/oauth2/token",
                AuthorizationClientId = "07FB4ADB-216C-48D1-BD90-E218129CEB4D",
                AuthorizationClientSecret = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJjaWQiOiIwN2ZiNGFkYi0yMTZjLTQ4ZDEtYmQ5MC1lMjE4MTI5Y2ViNGQiLCJjc2kiOiI3NzRmNzAxOC03YWRkLTRjYzktOGJjMy03YTczNWU0NWVjNTciLCJuYW1laWQiOiIxZWFmZTE5ZS01ZGQ3LTQ4MmMtODJmYi03OGUyMzE0ZTYxNDciLCJpc3MiOiJhcHAudnNzcHMudmlzdWFsc3R1ZGlvLmNvbSIsImF1ZCI6ImFwcC52c3Nwcy52aXN1YWxzdHVkaW8uY29tIiwibmJmIjoxNTI4OTk2NzU1LCJleHAiOjE2ODY3NjMxNTV9.wvvoxDeBSKH4QzXUyWUAfmDmLwC3Si1GTeI_4XSd5tZhwEU3dYNiEFIvAfCAWrt81NPMAXGkpNrH7oogkyoW8NtnzhBiAo8z0PE2Aw8cVQh95JlNH1XNhd3G7yv0wpzETVP6xfHRpJo1bYPVJAHg2d9PSEty_jMN8CdWUxf3R6VUxw-CCQkiV8STCTc-GKtRFoESV_gwg2KryEExRr3BCvq8vAKV8ThD-aES250oqyJ8twzIzkatE7INvmuxJeIY81X3twV6q4EpfEaiaQ7KHF7gpO_5zf5coAXX0L829Hm5qlpnME8548Q5aWTtkoC6fGg2ldKxGhIlM3IPhYTNqA",
                AuthorizationCallbackUrl = "https://hcc-devops-authorization.azurewebsites.net/api/auth",
                TokenEarlyExpiry = 10,
                AuthorizationUserId = "SolutionDeployConsole",
                AuthorizedScopes = new string[] { "vso.build_execute", "vso.release_manage" }
            };
        }
    }
}
