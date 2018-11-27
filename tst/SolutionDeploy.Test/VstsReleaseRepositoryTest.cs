namespace SolutionDeploy.Test
{
    using System;
    using System.Collections.Generic;
    using System.Security.Authentication;

    using Microsoft.TeamFoundation.Build.WebApi;
    using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
    using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;

    using NSubstitute;
    using Xunit;

    using SolutionDeploy.Core;
    using SolutionDeploy.Vsts;

    public class VstsReleaseRepositoryTest
    {
        IReleaseRepository sut;

        [Fact]
        public void Ctor_NullHttpClient_ThrowsException()
        {
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            Assert.Throws<ArgumentNullException>(() =>
                this.sut = new VstsReleaseRepository(null, authenticator, vstsConfig));
        }

        [Fact]
        public void Ctor_NullAuthenticator_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            VstsConfig vstsConfig = new VstsConfig();

            Assert.Throws<ArgumentNullException>(() =>
                this.sut = new VstsReleaseRepository(releaseClient, null, vstsConfig));
        }

        [Fact]
        public void Ctor_NullConfig_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();

            Assert.Throws<ArgumentNullException>(() =>
                this.sut = new VstsReleaseRepository(releaseClient, authenticator, null));
        }

        [Fact]
        public void GetReleaseId_NullService_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<ArgumentException>(() => this.sut.GetReleaseId(null, version: "someVersion"));
        }

        [Fact]
        public void GetReleaseId_Authenticates()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig
            {
                BaseUrl = @"https://hcc-devops.visualstudio.com/"
            };
            authenticator.Authenticate().Returns(new AuthenticationResult
            {
                Success = true,
                AccessToken = "foo"
            });

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            this.sut.GetReleaseId("someRelease", version: "someVersion");

            authenticator.Received().Authenticate();
        }

        [Fact]
        public void GetReleaseId_AuthenticationFails_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult());

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<AuthenticationException>(() =>
                this.sut.GetReleaseId("someRelease", "some Version"));
        }

        [Fact]
        public void GetReleaseId_AuthenticationSucceeds_GetsReleaseDefinitions()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            this.sut.GetReleaseId("someService", version: "someVersion");

            releaseClient.ReceivedWithAnyArgs().GetReleaseDefinitions(default(string), default(string));
        }

        [Fact]
        public void GetReleaseId_NoReleaseDefintions_ReturnsNull()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            string result = this.sut.GetReleaseId("someService", version: "someVersion");

            Assert.Null(result);
        }

        [Fact]
        public void GetReleaseId_WithVersion_GetsBuilds()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetReleaseDefinitions(Arg.Any<string>(), Arg.Any<string>()).Returns(GetTestReleaseDefinitions());

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            this.sut.GetReleaseId("someService", version: "someVersion");

            releaseClient.ReceivedWithAnyArgs().GetBuilds(default(int), default(string), default(string));
        }

        [Fact]
        public void GetReleaseId_WithBranch_GetsBuilds()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetReleaseDefinitions(Arg.Any<string>(), Arg.Any<string>()).Returns(GetTestReleaseDefinitions());

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            this.sut.GetReleaseId("someService", branch: "someBranch");

            releaseClient.ReceivedWithAnyArgs().GetBuilds(default(int), default(string), default(string), default(string));
        }

        [Fact]
        public void GetReleaseId_NoBuilds_ReturnsNull()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetReleaseDefinitions(Arg.Any<string>(), Arg.Any<string>()).Returns(GetTestReleaseDefinitions());

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            string result = this.sut.GetReleaseId("someService", version: "someVersion");

            Assert.Null(result);
        }

        [Fact]
        public void GetReleaseId_NoVersion_GetsLatestRelease()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetReleaseDefinitions(Arg.Any<string>(), Arg.Any<string>()).Returns(GetTestReleaseDefinitions());

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            this.sut.GetReleaseId("someService", version: null);

            releaseClient.Received().GetReleases(Arg.Any<int>(), Arg.Any<string>(), null, null);
        }

        [Fact]
        public void GetReleaseId_WithPrereqEnv_GetsReleaseWithPrereq()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetReleaseDefinitions(Arg.Any<string>(), Arg.Any<string>()).Returns(GetTestReleaseDefinitions());

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            this.sut.GetReleaseId("someService", version: null, prereqEnvironment: "someEnvironment");

            releaseClient.Received().GetReleases(Arg.Any<int>(), Arg.Any<string>(), null, Arg.Any<int>());
        }

        [Fact]
        public void GetReleaseId_GetsBuilds_GetsReleases()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetReleaseDefinitions(Arg.Any<string>(), Arg.Any<string>()).Returns(GetTestReleaseDefinitions());
            releaseClient.GetBuilds(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>()).Returns(GetTestBuilds());

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            this.sut.GetReleaseId("someService", version: "someVersion");

            releaseClient.ReceivedWithAnyArgs().GetReleases(default(int), default(string), default(int));
        }

        [Fact]
        public void GetReleaseId_NoReleases_ReturnsNull()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetReleaseDefinitions(Arg.Any<string>(), Arg.Any<string>()).Returns(GetTestReleaseDefinitions());
            releaseClient.GetBuilds(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>()).Returns(GetTestBuilds());

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            string result = this.sut.GetReleaseId("someService", version: "someVersion");

            Assert.Null(result);
        }

        [Fact]
        public void GetReleaseId_GetsReleases_ReturnsReleaseId()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetReleaseDefinitions(Arg.Any<string>(), Arg.Any<string>()).Returns(GetTestReleaseDefinitions());
            releaseClient.GetBuilds(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>()).Returns(GetTestBuilds());
            releaseClient.GetReleases(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>()).Returns(GetTestReleases());

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            string result = this.sut.GetReleaseId("someService", version: "someVersion");

            Assert.NotNull(result);
        }

        [Fact]
        public void GetEnvironmentId_NullReleaseId_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<ArgumentException>(() => this.sut.GetReleaseEnvironmentId(null, "someEnvironment"));
        }

        [Fact]
        public void GetEnvironmentId_NullEnvironment_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<ArgumentException>(() => this.sut.GetReleaseEnvironmentId("999", null));
        }

        [Fact]
        public void GetEnvironmentId_Authenticates()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig
            {
                BaseUrl = @"https://hcc-devops.visualstudio.com/"
            };
            authenticator.Authenticate().Returns(new AuthenticationResult
            {
                Success = true,
                AccessToken = "foo"
            });

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            this.sut.GetReleaseEnvironmentId("999", "someEnvironment");

            authenticator.Received().Authenticate();
        }

        [Fact]
        public void GetEnvironmentId_ReleaseIdNaN_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<ArgumentException>(() => this.sut.GetReleaseEnvironmentId("someRelease", "999"));
        }

        [Fact]
        public void GetEnvironmentId_AuthenticationFails_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult());

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<AuthenticationException>(() =>
                this.sut.GetReleaseEnvironmentId("999", "someEnvironment"));
        }

        [Fact]
        public void GetEnvironmentId_AuthenticationSucceeds_GetsRelease()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            this.sut.GetReleaseEnvironmentId("999", "someEnvironment");

            releaseClient.ReceivedWithAnyArgs().GetRelease(default(int), default(string));
        }

        [Fact]
        public void GetEnvironmentId_NoRelease_ReturnsNull()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            string result = this.sut.GetReleaseEnvironmentId("999", "someEnvironment");

            Assert.Null(result);
        }

        [Fact]
        public void GetEnvironmentId_NoEnvironment_ReturnsNull()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetRelease(Arg.Any<int>(), Arg.Any<string>())
                .Returns(GetTestRelease(EnvironmentStatus.Succeeded));

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            string result = this.sut.GetReleaseEnvironmentId("999", "someEnvironment");

            Assert.Null(result);
        }

        [Fact]
        public void GetEnvironmentId_GetsEnvironment_ReturnsEnvironmentId()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetRelease(Arg.Any<int>(), Arg.Any<string>())
                .Returns(GetTestRelease(EnvironmentStatus.Succeeded));

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            string result = this.sut.GetReleaseEnvironmentId("999", "Environment1");

            Assert.NotNull(result);
        }

        [Fact]
        public void UpdateReleaseEnvironment_NullReleaseId_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<ArgumentException>(() => this.sut.UpdateReleaseEnvironment(null, "999"));
        }

        [Fact]
        public void UpdateReleaseEnvironment_NullEnvironmentId_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<ArgumentException>(() => this.sut.UpdateReleaseEnvironment("123", null));
        }

        [Fact]
        public void UpdateReleaseEnvironment_ReleaseIdNaN_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<ArgumentException>(() => this.sut.UpdateReleaseEnvironment("someRelease", "999"));
        }

        [Fact]
        public void UpdateReleaseEnvironment_EnvironmentIdNaN_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<ArgumentException>(() => this.sut.UpdateReleaseEnvironment("123", "someEnvironment"));
        }

        [Fact]
        public void UpdateReleaseEnvironment_AuthenticationFails_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult());

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<AuthenticationException>(() =>
                this.sut.UpdateReleaseEnvironment("123", "999"));
        }

        [Fact]
        public void UpdateReleaseEnvironment_AuthenticationSucceeds_UpdatesEnvironment()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            this.sut.UpdateReleaseEnvironment("123", "999");

            releaseClient.ReceivedWithAnyArgs()
                .UpdateReleaseEnvironment(default(int), default(int), default(string));
        }

        [Fact]
        public void GetReleaseEnvironmentStatus_NullReleaseId_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<ArgumentException>(() =>
                this.sut.GetReleaseEnvironmentStatus(null, "someEnvironment"));
        }

        [Fact]
        public void GetReleaseEnvironmentStatus_NullEnvironmentId_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<ArgumentException>(() =>
                this.sut.GetReleaseEnvironmentStatus("someRelease", null));
        }

        [Fact]
        public void GetReleaseEnvironmentStatus_ReleaseIdNaN_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<ArgumentException>(() =>
                this.sut.GetReleaseEnvironmentStatus("someReleaseId", "999"));
        }

        [Fact]
        public void GetReleaseEnvironmentStatus_EnvironmentIdNaN_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<ArgumentException>(() =>
                this.sut.GetReleaseEnvironmentStatus("123", "someEnvironment"));
        }

        [Fact]
        public void GetReleaseEnvironmentStatus_AuthenticationFails_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult());

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<AuthenticationException>(() =>
                this.sut.GetReleaseEnvironmentStatus("123", "999"));
        }

        [Fact]
        public void GetReleaseEnvironmentStatus_AuthenticationSucceeds_GetsRelease()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            this.sut.GetReleaseEnvironmentStatus("123", "999");

            releaseClient.ReceivedWithAnyArgs().GetRelease(default(int), default(string));
        }

        [Fact]
        public void GetReleaseEnvironmentStatus_DoesntGetRelease_ReturnsUnknown()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetRelease(Arg.Any<int>(), Arg.Any<string>()).Returns((Release)null);

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            SolutionDeploy.Core.DeploymentStatus result = this.sut.GetReleaseEnvironmentStatus("123", "999");

            Assert.Equal(SolutionDeploy.Core.DeploymentStatus.Unknown, result);
        }

        [Fact]
        public void GetReleaseEnvironmentStatus_GetsReleaseNoEnvironment_ReturnsUnknown()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetRelease(Arg.Any<int>(), Arg.Any<string>())
                .Returns(this.GetTestRelease(EnvironmentStatus.Succeeded));

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            SolutionDeploy.Core.DeploymentStatus result = this.sut.GetReleaseEnvironmentStatus("123", "999");

            Assert.Equal(SolutionDeploy.Core.DeploymentStatus.Unknown, result);
        }

        [Fact]
        public void GetReleaseEnvironmentStatus_ReleaseInProgress_ChecksForApprovals()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetRelease(Arg.Any<int>(), Arg.Any<string>()).Returns(GetTestRelease(EnvironmentStatus.InProgress));

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            SolutionDeploy.Core.DeploymentStatus result = this.sut.GetReleaseEnvironmentStatus("123", "123");

            releaseClient.ReceivedWithAnyArgs().GetApprovals(default(int), default(string));
        }

        [Fact]
        public void GetReleaseEnvironmentStatus_ReleaseHasNoApprovals_ReturnsInProgress()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetRelease(Arg.Any<int>(), Arg.Any<string>()).Returns(GetTestRelease(EnvironmentStatus.InProgress));
            releaseClient.GetApprovals(Arg.Any<int>(), Arg.Any<string>()).Returns(new List<ReleaseApproval>());

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            SolutionDeploy.Core.DeploymentStatus result = this.sut.GetReleaseEnvironmentStatus("123", "123");

            Assert.Equal(SolutionDeploy.Core.DeploymentStatus.InProgress, result);
        }

        [Fact]
        public void GetReleaseEnvironmentStatus_ReleaseHasApprovals_ReturnsPendingApproval()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetRelease(Arg.Any<int>(), Arg.Any<string>()).Returns(GetTestRelease(EnvironmentStatus.InProgress));
            releaseClient.GetApprovals(Arg.Any<int>(), Arg.Any<string>()).Returns(
                new List<ReleaseApproval>
                {
                    new ReleaseApproval()
                });

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            SolutionDeploy.Core.DeploymentStatus result = this.sut.GetReleaseEnvironmentStatus("123", "123");

            Assert.Equal(SolutionDeploy.Core.DeploymentStatus.PendingApproval, result);
        }

        [Theory]
        [InlineData(EnvironmentStatus.Canceled, SolutionDeploy.Core.DeploymentStatus.Cancelled)]
        [InlineData(EnvironmentStatus.InProgress, SolutionDeploy.Core.DeploymentStatus.InProgress)]
        [InlineData(EnvironmentStatus.NotStarted, SolutionDeploy.Core.DeploymentStatus.NotStarted)]
        [InlineData(EnvironmentStatus.PartiallySucceeded, SolutionDeploy.Core.DeploymentStatus.Failed)]
        [InlineData(EnvironmentStatus.Queued, SolutionDeploy.Core.DeploymentStatus.Queued)]
        [InlineData(EnvironmentStatus.Rejected, SolutionDeploy.Core.DeploymentStatus.Failed)]
        [InlineData(EnvironmentStatus.Scheduled, SolutionDeploy.Core.DeploymentStatus.Queued)]
        [InlineData(EnvironmentStatus.Succeeded, SolutionDeploy.Core.DeploymentStatus.Succeeded)]
        [InlineData(EnvironmentStatus.Undefined, SolutionDeploy.Core.DeploymentStatus.Unknown)]
        [InlineData((EnvironmentStatus.Undefined) - 1, SolutionDeploy.Core.DeploymentStatus.Unknown)]
        internal void GetReleaseEnvironmentStatus_GetsEnvironment_ReturnsStatus(
            EnvironmentStatus envStatus,
            SolutionDeploy.Core.DeploymentStatus deployStatus)
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult { Success = true });
            releaseClient.GetRelease(Arg.Any<int>(), Arg.Any<string>())
                .Returns(this.GetTestRelease(envStatus));

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            SolutionDeploy.Core.DeploymentStatus result = this.sut.GetReleaseEnvironmentStatus("123", "123");

            Assert.Equal(deployStatus, result);
        }

        [Fact]
        public void UpdateApproval_NullReleaseId_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<ArgumentException>(() =>
                this.sut.UpdateApproval(null));
        }

        [Fact]
        public void UpdateApproval_ReleaseIdNaN_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<ArgumentException>(() =>
                this.sut.UpdateApproval("someReleaseId"));
        }

        [Fact]
        public void UpdateApproval_AuthenticationFails_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult());

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<AuthenticationException>(() =>
                this.sut.UpdateApproval("123"));
        }

        [Fact]
        public void UpdateApproval_AuthenticationSucceeds_GetsApprovals()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            authenticator.Authenticate().Returns(new AuthenticationResult()
            {
                Success = true
            });

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            this.sut.UpdateApproval("123");

            releaseClient.ReceivedWithAnyArgs().GetApprovals(Arg.Any<int>(), Arg.Any<string>());
        }

        [Fact]
        public void UpdateApproval_DoesntGetApprovals_Returns()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            releaseClient.GetApprovals(Arg.Any<int>(), Arg.Any<string>())
                .Returns(new List<ReleaseApproval>());

            authenticator.Authenticate().Returns(new AuthenticationResult()
            {
                Success = true
            });

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            this.sut.UpdateApproval("123");

            releaseClient.DidNotReceiveWithAnyArgs().UpdateApproval(Arg.Any<int>(), Arg.Any<string>());
        }

        [Fact]
        public void UpdateApproval_GetsApprovals_UpdatesApproval()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            releaseClient.GetApprovals(Arg.Any<int>(), Arg.Any<string>())
                .Returns(new List<ReleaseApproval>
                    {
                        new ReleaseApproval{Id = 123}
                    });

            authenticator.Authenticate().Returns(new AuthenticationResult()
            {
                Success = true
            });

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            this.sut.UpdateApproval("123");

            releaseClient.ReceivedWithAnyArgs(1).UpdateApproval(Arg.Any<int>(), Arg.Any<string>());
        }

        [Fact]
        public void GetLatest_ThrowsException()
        {
            IVstsReleaseClient releaseClient = Substitute.For<IVstsReleaseClient>();
            IAuthenticator authenticator = Substitute.For<IAuthenticator>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsReleaseRepository(releaseClient, authenticator, vstsConfig);

            Assert.Throws<NotImplementedException>(() => this.sut.GetLatest("someService"));
        }

        private IEnumerable<ReleaseDefinition> GetTestReleaseDefinitions()
        {
            IList<ReleaseDefinition> releaseDefinitions = new List<ReleaseDefinition>
            {
                new ReleaseDefinition{
                    Artifacts = new List<Artifact>
                    {
                        new Artifact
                        {
                            IsPrimary = true,
                            DefinitionReference = new Dictionary<string, ArtifactSourceReference>
                            {
                                {"definition", new ArtifactSourceReference { Id = "999" }}
                            }
                        }
                    },
                    Environments = new List<ReleaseDefinitionEnvironment>
                    {
                        new ReleaseDefinitionEnvironment
                        {
                            Id = 1,
                            Name = "someEnvironment"
                        }
                    }
                }
            };

            return releaseDefinitions;
        }


        private IEnumerable<Build> GetTestBuilds()
        {
            IList<Build> builds = new List<Build>
            {
                new Build()
            };

            return builds;
        }

        private IEnumerable<Release> GetTestReleases()
        {
            IList<Release> releases = new List<Release>
            {
                new Release
                {
                    Id = 999
                }
            };

            return releases;
        }

        private Release GetTestRelease(EnvironmentStatus status)
        {
            Release release = new Release
            {
                Environments = new List<ReleaseEnvironment>
                {
                    new ReleaseEnvironment
                    {
                        Id = 123,
                        Name = "Environment1",
                        Status = status
                    }
                }
            };

            return release;
        }

        private ReleaseEnvironment GetTestEnvironment(EnvironmentStatus status)
        {
            ReleaseEnvironment releaseEnvironment = new ReleaseEnvironment
            {
                Status = status
            };

            return releaseEnvironment;
        }
    }
}
