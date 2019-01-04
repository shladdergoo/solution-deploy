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
                BaseUrl = "",
                ProjectName = "CI",
                TokenUrl = "https://app.vssps.visualstudio.com/oauth2/token",
                AuthorizationClientId = "",
                AuthorizationClientSecret = "",
                AuthorizationCallbackUrl = "",
                TokenEarlyExpiry = 10,
                AuthorizationUserId = "SolutionDeployConsole",
                AuthorizedScopes = new string[] { "vso.build_execute", "vso.release_manage" }
            };
        }
    }
}
