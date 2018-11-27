namespace SolutionDeploy.Test
{
    using System;

    using NSubstitute;
    using Xunit;

    using SolutionDeploy.Core;
    using SolutionDeploy.Vsts;

    public class VstsDeploymentExecutorTest
    {
        IServiceDeploymentExecutor sut;

        [Fact]
        public void Ctor_NullReleaseRepository_ThrowsException()
        {
            VstsConfig vstsConfig = new VstsConfig();

            Assert.Throws<ArgumentNullException>(() => this.sut =
                new VstsDeploymentExecutor(null, vstsConfig));
        }

        [Fact]
        public void Ctor_NullVstsConfig_ThrowsException()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();

            Assert.Throws<ArgumentNullException>(() =>
                this.sut = new VstsDeploymentExecutor(releaseRepository, null));
        }

        [Fact]
        public void Deploy_NullServiceName_ThrowsException()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            Assert.Throws<ArgumentException>(() => this.sut.Deploy(null, environment: "someEnvironment", version: "someVersion"));
        }

        [Fact]
        public void Deploy_NullEnvironment_ThrowsException()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            Assert.Throws<ArgumentException>(() => this.sut.Deploy("someService", environment: null, version: "someVersion"));
        }

        [Fact]
        public void Deploy_GetsReleaseId_GetsEnvironmentId()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            releaseRepository.GetReleaseId(Arg.Any<string>(), Arg.Any<string>()).Returns("someId");

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            sut.Deploy("someService", environment: "someEnvironment", version: "someVersion");

            releaseRepository.ReceivedWithAnyArgs().GetReleaseEnvironmentId(
                default(string), default(string));
        }

        [Fact]
        public void Deploy_DoesntGetReleaseId_DoesntGetEnvironmentId()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            releaseRepository.GetReleaseId(Arg.Any<string>(), Arg.Any<string>()).Returns((string)null);

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            sut.Deploy("someService", environment: "someEnvironment", version: "someVersion");

            releaseRepository.DidNotReceiveWithAnyArgs().GetReleaseEnvironmentId(
                default(string), default(string));
        }

        [Fact]
        public void Deploy_DoesntGetEnvironmentId_Returns()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            releaseRepository.GetReleaseId(Arg.Any<string>(), Arg.Any<string>()).Returns("123");
            releaseRepository.GetReleaseEnvironmentId(Arg.Any<string>(), Arg.Any<string>())
                .Returns((string)null);

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            sut.Deploy("someService", environment: "someEnvironment", version: "someVersion");

            releaseRepository.DidNotReceiveWithAnyArgs()
                .UpdateReleaseEnvironment(default(string), default(string));
        }

        [Fact]
        public void Deploy_GetsEnvironmentId_UpdatesEnvironment()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            releaseRepository.GetReleaseId(Arg.Any<string>(), Arg.Any<string>()).Returns("123");
            releaseRepository.GetReleaseEnvironmentId(Arg.Any<string>(), Arg.Any<string>()).Returns("123");

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            sut.Deploy("someService", environment: "someEnvironment", version: "someVersion");

            releaseRepository.ReceivedWithAnyArgs().UpdateReleaseEnvironment(default(string), default(string));
        }

        [Fact]
        public void Deploy_UpdatesEnvironment_ReturnsTrue()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            releaseRepository.GetReleaseId(Arg.Any<string>(), Arg.Any<string>()).Returns("123");
            releaseRepository.GetReleaseEnvironmentId(Arg.Any<string>(), Arg.Any<string>()).Returns("123");

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            bool retval = sut.Deploy("someService", environment: "someEnvironment", version: "someVersion");

            Assert.True(retval);
        }

        [Fact]
        public void Deploy_WhatIf_DoesntUpdateEnvironment()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            releaseRepository.GetReleaseId(Arg.Any<string>(), Arg.Any<string>()).Returns("123");
            releaseRepository.GetReleaseEnvironmentId(Arg.Any<string>(), Arg.Any<string>()).Returns("123");

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig, new Options { WhatIf = true });

            sut.Deploy("someService", environment: "someEnvironment", version: "someVersion");

            releaseRepository.DidNotReceiveWithAnyArgs().UpdateReleaseEnvironment(default(string), default(string));
        }

        [Fact]
        public void GetDeploymentStatus_NullServiceName_ThrowsException()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            Assert.Throws<ArgumentException>(() => this.sut
                .GetDeploymentStatus(null, "someEnvironment", "someVersion"));
        }

        [Fact]
        public void GetDeploymentStatus_NullEnvironment_ThrowsException()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            Assert.Throws<ArgumentException>(() => this.sut
                .GetDeploymentStatus("someService", null, "someVersion"));
        }

        [Fact]
        public void GetDeploymentStatus_DoesntGetReleaseId_ReturnsUnknown()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            releaseRepository.GetReleaseId(Arg.Any<string>(), Arg.Any<string>())
                .Returns((string)null);

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            DeploymentStatus result = this.sut.GetDeploymentStatus("someService", "someEnvironment", "someVersion");

            Assert.Equal(DeploymentStatus.Unknown, result);
        }

        [Fact]
        public void GetDeploymentStatus_GetsReleaseId_GetsEnvironmentId()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            releaseRepository.GetReleaseId(Arg.Any<string>(), Arg.Any<string>())
                .Returns("123");

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            this.sut.GetDeploymentStatus("someService", "someEnvironment", "someVersion");

            releaseRepository.ReceivedWithAnyArgs().GetReleaseEnvironmentId(default(string), default(string));
        }

        [Fact]
        public void GetDeploymentStatus_DoesntGetEnvironmentId_ReturnsUnknown()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            releaseRepository.GetReleaseId(Arg.Any<string>(), Arg.Any<string>())
                .Returns("123");
            releaseRepository.GetReleaseEnvironmentId(Arg.Any<string>(), Arg.Any<string>())
                .Returns((string)null);

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            DeploymentStatus result = this.sut.GetDeploymentStatus("someService", "someEnvironment", "someVersion");

            Assert.Equal(DeploymentStatus.Unknown, result);
        }

        [Fact]
        public void GetDeploymentStatus_GetsEnvironmentId_GetsEnvironmentStatus()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            releaseRepository.GetReleaseId(Arg.Any<string>(), Arg.Any<string>())
                .Returns("123");
            releaseRepository.GetReleaseEnvironmentId(Arg.Any<string>(), Arg.Any<string>())
                .Returns("123");

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            this.sut.GetDeploymentStatus("someService", "someEnvironment", "someVersion");

            releaseRepository.ReceivedWithAnyArgs().GetReleaseEnvironmentStatus(
                default(string), default(string));
        }

        [Fact]
        public void GetDeploymentStatus_GetsEnvironmentStatus_ReturnsStatus()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            releaseRepository.GetReleaseId(Arg.Any<string>(), Arg.Any<string>())
                .Returns("123");
            releaseRepository.GetReleaseEnvironmentId(Arg.Any<string>(), Arg.Any<string>())
                .Returns("123");
            releaseRepository.GetReleaseEnvironmentStatus(Arg.Any<string>(), Arg.Any<string>())
                .Returns(DeploymentStatus.Succeeded);

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            DeploymentStatus result = this.sut.GetDeploymentStatus(
                    "someService", "someEnvironment", "someVersion");

            Assert.Equal(DeploymentStatus.Succeeded, result);
        }

        [Fact]
        public void GetDeploymentStatus_PendingApproval_ApprovesRelease()
        {
            IReleaseRepository releaseRepository = Substitute.For<IReleaseRepository>();
            VstsConfig vstsConfig = new VstsConfig();

            releaseRepository.GetReleaseId(Arg.Any<string>(), Arg.Any<string>())
                .Returns("123");
            releaseRepository.GetReleaseEnvironmentId(Arg.Any<string>(), Arg.Any<string>())
                .Returns("123");
            releaseRepository.GetReleaseEnvironmentStatus(Arg.Any<string>(), Arg.Any<string>())
                .Returns(DeploymentStatus.PendingApproval);

            this.sut = new VstsDeploymentExecutor(releaseRepository, vstsConfig);

            this.sut.GetDeploymentStatus("someService", "someEnvironment", "someVersion");

            releaseRepository.ReceivedWithAnyArgs().UpdateApproval(default(string));
        }
    }
}
