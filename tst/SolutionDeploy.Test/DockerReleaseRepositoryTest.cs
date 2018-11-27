namespace SolutionDeploy.Test
{
    using System;

    using NSubstitute;
    using Xunit;

    using SolutionDeploy.Core;
    using SolutionDeploy.Docker;

    public class DockerReleaseRepositoryTest
    {
        private IReleaseRepository sut;

        [Fact]
        public void Ctor_NullRegistry_ThrowsException()
        {
            IHttpClient httpClient = Substitute.For<IHttpClient>();

            Assert.Throws<ArgumentException>(() => this.sut = new DockerReleaseRepository(null, httpClient));
        }

        [Fact]
        public void Ctor_NullHttpClient_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => this.sut = new DockerReleaseRepository("someRegistry", null));
        }

        [Fact]
        public void GetReleaseId_NullService_ThrowsException()
        {
            IHttpClient httpClient = Substitute.For<IHttpClient>();

            this.sut = new DockerReleaseRepository("someRepository", httpClient);

            Assert.Throws<ArgumentException>(() => this.sut.GetReleaseId(null));
        }

        [Fact]
        public void GetReleaseId_NullVersion_ReturnsReleaseId()
        {
            IHttpClient httpClient = Substitute.For<IHttpClient>();

            this.sut = new DockerReleaseRepository("someRepository", httpClient);

            string result = this.sut.GetReleaseId("someService");

            Assert.Equal(@"someRepository/someService", result);
        }

        [Fact]
        public void GetReleaseId_HasVersion_ReturnsReleaseId()
        {
            IHttpClient httpClient = Substitute.For<IHttpClient>();

            this.sut = new DockerReleaseRepository("someRepository", httpClient);

            string result = this.sut.GetReleaseId("someService", "someTag");

            Assert.Equal(@"someRepository/someService:someTag", result);
        }

        [Fact]
        public void GetReleaseEnvironmentId_ThrowsException()
        {
            IHttpClient httpClient = Substitute.For<IHttpClient>();

            this.sut = new DockerReleaseRepository("someRepository", httpClient);

            Assert.Throws<NotImplementedException>(() => this.sut.GetReleaseEnvironmentId("someReleaseId", "someEnvironment"));
        }

        [Fact]
        public void GetReleaseEnvironmentStatus_ThrowsException()
        {
            IHttpClient httpClient = Substitute.For<IHttpClient>();

            this.sut = new DockerReleaseRepository("someRepository", httpClient);

            Assert.Throws<NotImplementedException>(() => this.sut.GetReleaseEnvironmentStatus("someReleaseId", "someEnvId"));
        }

       [Fact]
        public void UpdateApproval_ThrowsException()
        {
            IHttpClient httpClient = Substitute.For<IHttpClient>();

            this.sut = new DockerReleaseRepository("someRepository", httpClient);

            Assert.Throws<NotImplementedException>(() => this.sut.UpdateApproval("someReleaseId"));
        }

       [Fact]
        public void UpdateReleaseEnvironment_ThrowsException()
        {
            IHttpClient httpClient = Substitute.For<IHttpClient>();

            this.sut = new DockerReleaseRepository("someRepository", httpClient);

            Assert.Throws<NotImplementedException>(() => this.sut.UpdateReleaseEnvironment("someReleaseId", "SomeEnvId"));
        }
    }
}
