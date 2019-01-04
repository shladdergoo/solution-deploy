namespace SolutionDeploy.Test.Integration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Newtonsoft.Json;
    using Xunit;

    using Microsoft.TeamFoundation.Build.WebApi;
    using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;

    using SolutionDeploy.Core;
    using SolutionDeploy.Vsts;

    public class VstsSyncReleaseClientTest
    {
        private const string TokenFilename = "solutiondeploy_tokens.json";
        string fullTokenFilename =
            Path.Combine(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"), TokenFilename);
        IVstsReleaseClient sut;

        [Fact]
        public void GetReleaseDefinitions_Succeeds()
        {
            VstsConfig vstsConfig = GetVstsConfig();

            IHttpClient httpClient = new HttpClient();
            ITokenRepository repository = new JsonFileTokenRepository(new FileSystem());
            IAuthenticator authenticator = new VstsOAuthAuthenticator(httpClient, repository, vstsConfig);

            File.Copy($"..//..//..//testdata//{TokenFilename}", fullTokenFilename, true);

            AuthenticationResult authResult = authenticator.Authenticate();

            File.Copy(fullTokenFilename, $"..//..//..//testdata//{TokenFilename}", true);

            this.sut = new VstsSyncReleaseClient(vstsConfig);

            IEnumerable<ReleaseDefinition> result = this.sut.GetReleaseDefinitions(
                "samplepackage",
                authResult.AccessToken);

            Assert.NotNull(result);

            SaveResultData(result, "../../../testdata/GetReleaseDefinitions.json");
        }

        [Fact]
        public void GetReleaseDefinition_Succeeds()
        {
            VstsConfig vstsConfig = GetVstsConfig();

            IHttpClient httpClient = new HttpClient();
            ITokenRepository repository = new JsonFileTokenRepository(new FileSystem());
            IAuthenticator authenticator = new VstsOAuthAuthenticator(httpClient, repository, vstsConfig);

            File.Copy($"..//..//..//testdata//{TokenFilename}", fullTokenFilename, true);

            AuthenticationResult authResult = authenticator.Authenticate();

            File.Copy(fullTokenFilename, $"..//..//..//testdata//{TokenFilename}", true);

            this.sut = new VstsSyncReleaseClient(vstsConfig);

            ReleaseDefinition result = this.sut.GetReleaseDefinition(1, authResult.AccessToken);

            Assert.NotNull(result);

            SaveResultData(result, "../../../testdata/GetReleaseDefinition.json");
        }

        [Fact]
        public void GetReleases_AllReleases_Succeeds()
        {
            VstsConfig vstsConfig = GetVstsConfig();

            IHttpClient httpClient = new HttpClient();
            ITokenRepository repository = new JsonFileTokenRepository(new FileSystem());
            IAuthenticator authenticator = new VstsOAuthAuthenticator(httpClient, repository, vstsConfig);

            File.Copy($"..//..//..//testdata//{TokenFilename}", fullTokenFilename, true);

            AuthenticationResult authResult = authenticator.Authenticate();

            File.Copy(fullTokenFilename, $"..//..//..//testdata//{TokenFilename}", true);

            this.sut = new VstsSyncReleaseClient(vstsConfig);

            IEnumerable<Release> result = this.sut.GetReleases(1, authResult.AccessToken);

            Assert.NotNull(result);
            Assert.True(result.Count() > 0);

            SaveResultData(result, "../../../testdata/GetReleases_AllReleases.json");
        }

        [Fact]
        public void GetReleases_ArtifactVersionId_Succeeds()
        {
            VstsConfig vstsConfig = GetVstsConfig();

            IHttpClient httpClient = new HttpClient();
            ITokenRepository repository = new JsonFileTokenRepository(new FileSystem());
            IAuthenticator authenticator = new VstsOAuthAuthenticator(httpClient, repository, vstsConfig);

            File.Copy($"..//..//..//testdata//{TokenFilename}", fullTokenFilename, true);

            AuthenticationResult authResult = authenticator.Authenticate();

            File.Copy(fullTokenFilename, $"..//..//..//testdata//{TokenFilename}", true);

            this.sut = new VstsSyncReleaseClient(vstsConfig);

            IEnumerable<Release> result = this.sut.GetReleases(
                1, authResult.AccessToken, 15);

            Assert.NotNull(result);
            Assert.True(result.Count() > 0);

            SaveResultData(result, "../../../testdata/GetReleases_ArtifactVersionId.json");
        }

        [Fact]
        public void GetReleases_AllReleasesWithPrereqEnv_Succeeds()
        {
            VstsConfig vstsConfig = GetVstsConfig();

            IHttpClient httpClient = new HttpClient();
            ITokenRepository repository = new JsonFileTokenRepository(new FileSystem());
            IAuthenticator authenticator = new VstsOAuthAuthenticator(httpClient, repository, vstsConfig);

            File.Copy($"..//..//..//testdata//{TokenFilename}", fullTokenFilename, true);

            AuthenticationResult authResult = authenticator.Authenticate();

            File.Copy(fullTokenFilename, $"..//..//..//testdata//{TokenFilename}", true);

            this.sut = new VstsSyncReleaseClient(vstsConfig);

            IEnumerable<Release> result =
                this.sut.GetReleases(1, authResult.AccessToken, null, 1);

            Assert.NotNull(result);
            Assert.True(result.Count() > 0);

            SaveResultData(result, "../../../testdata/GetReleases_AllReleasesWithPrereqEnv.json");
        }


        [Fact]
        public void GetRelease_Succeeds()
        {
            VstsConfig vstsConfig = GetVstsConfig();

            IHttpClient httpClient = new HttpClient();
            ITokenRepository repository = new JsonFileTokenRepository(new FileSystem());
            IAuthenticator authenticator = new VstsOAuthAuthenticator(httpClient, repository, vstsConfig);

            File.Copy($"..//..//..//testdata//{TokenFilename}", fullTokenFilename, true);

            AuthenticationResult authResult = authenticator.Authenticate();

            File.Copy(fullTokenFilename, $"..//..//..//testdata//{TokenFilename}", true);

            this.sut = new VstsSyncReleaseClient(vstsConfig);

            Release result = this.sut.GetRelease(11, authResult.AccessToken);

            Assert.NotNull(result);

            SaveResultData(result, "../../../testdata/GetRelease.json");
        }

        [Fact]
        public void GetBuilds_BuildNumber_Succeeds()
        {
            VstsConfig vstsConfig = GetVstsConfig();

            IHttpClient httpClient = new HttpClient();
            ITokenRepository repository = new JsonFileTokenRepository(new FileSystem());
            IAuthenticator authenticator = new VstsOAuthAuthenticator(httpClient, repository, vstsConfig);

            File.Copy($"..//..//..//testdata//{TokenFilename}", fullTokenFilename, true);

            AuthenticationResult authResult = authenticator.Authenticate();

            File.Copy(fullTokenFilename, $"..//..//..//testdata//{TokenFilename}", true);

            this.sut = new VstsSyncReleaseClient(vstsConfig);

            IEnumerable<Build> result =
                this.sut.GetBuilds(1, authResult.AccessToken, buildNumber: "1.1.11");

            Assert.NotNull(result);
            Assert.True(result.Count() > 0);

            SaveResultData(result, "../../../testdata/GetBuilds_BuildNum.json");
        }

        [Fact]
        public void GetBuilds_Branch_Succeeds()
        {
            VstsConfig vstsConfig = GetVstsConfig();

            IHttpClient httpClient = new HttpClient();
            ITokenRepository repository = new JsonFileTokenRepository(new FileSystem());
            IAuthenticator authenticator = new VstsOAuthAuthenticator(httpClient, repository, vstsConfig);

            File.Copy($"..//..//..//testdata//{TokenFilename}", fullTokenFilename, true);

            AuthenticationResult authResult = authenticator.Authenticate();

            File.Copy(fullTokenFilename, $"..//..//..//testdata//{TokenFilename}", true);

            this.sut = new VstsSyncReleaseClient(vstsConfig);

            IEnumerable<Build> result =
                this.sut.GetBuilds(
                    1, authResult.AccessToken, branch: "refs/heads/master");

            Assert.NotNull(result);
            Assert.True(result.Count() > 0);

            SaveResultData(result, "../../../testdata/GetBuilds_Branch.json");
        }

        [Fact]
        public void UpdateReleaseEnvironment_Succeeds()
        {
            VstsConfig vstsConfig = GetVstsConfig();

            IHttpClient httpClient = new HttpClient();
            ITokenRepository repository = new JsonFileTokenRepository(new FileSystem());
            IAuthenticator authenticator = new VstsOAuthAuthenticator(httpClient, repository, vstsConfig);

            File.Copy($"..//..//..//testdata//{TokenFilename}", fullTokenFilename, true);

            AuthenticationResult authResult = authenticator.Authenticate();

            File.Copy(fullTokenFilename, $"..//..//..//testdata//{TokenFilename}", true);

            this.sut = new VstsSyncReleaseClient(vstsConfig);

            ReleaseEnvironment result = this.sut.UpdateReleaseEnvironment(11, 11, authResult.AccessToken);

            Assert.NotNull(result);
            Assert.Equal(EnvironmentStatus.Queued, result.Status);
        }

        [Fact]
        public void GetApprovals_Succeeds()
        {
            VstsConfig vstsConfig = GetVstsConfig();

            IHttpClient httpClient = new HttpClient();
            ITokenRepository repository = new JsonFileTokenRepository(new FileSystem());
            IAuthenticator authenticator = new VstsOAuthAuthenticator(httpClient, repository, vstsConfig);

            File.Copy($"..//..//..//testdata//{TokenFilename}", fullTokenFilename, true);

            AuthenticationResult authResult = authenticator.Authenticate();

            File.Copy(fullTokenFilename, $"..//..//..//testdata//{TokenFilename}", true);

            this.sut = new VstsSyncReleaseClient(vstsConfig);

            IEnumerable<ReleaseApproval> result = this.sut.GetApprovals(11, authResult.AccessToken);

            Assert.NotNull(result);

            SaveResultData(result, "../../../testdata/GetApprovals.json");
        }

        [Fact]
        public void UpdateApproval_Succeeds()
        {
            VstsConfig vstsConfig = GetVstsConfig();

            IHttpClient httpClient = new HttpClient();
            ITokenRepository repository = new JsonFileTokenRepository(new FileSystem());
            IAuthenticator authenticator = new VstsOAuthAuthenticator(httpClient, repository, vstsConfig);

            File.Copy($"..//..//..//testdata//{TokenFilename}", fullTokenFilename, true);

            AuthenticationResult authResult = authenticator.Authenticate();

            File.Copy(fullTokenFilename, $"..//..//..//testdata//{TokenFilename}", true);

            this.sut = new VstsSyncReleaseClient(vstsConfig);

            ReleaseApproval result = this.sut.UpdateApproval(123, authResult.AccessToken);

            Assert.NotNull(result);
            Assert.Equal(ApprovalStatus.Approved, result.Status);

            SaveResultData(result, "../../../testdata/UpdateApproval.json");
        }

        private static void SaveResultData(object result, string filename)
        {
            File.Delete(filename);

            StreamWriter streamWriter = new StreamWriter(File.OpenWrite(filename));

            using (JsonTextWriter textWriter = new JsonTextWriter(streamWriter))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(textWriter, result);
                textWriter.Flush();
            }
        }

        private static T LoadTestData<T>(string filename)
        {
            StreamReader streamReader = new StreamReader(File.OpenRead(filename));

            T testData = default(T);
            using (JsonTextReader textReader = new JsonTextReader(streamReader))
            {
                JsonSerializer serializer = new JsonSerializer();
                testData = serializer.Deserialize<T>(textReader);
            }

            return testData;
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
