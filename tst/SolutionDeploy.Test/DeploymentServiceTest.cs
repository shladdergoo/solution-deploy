namespace SolutionDeploy.Test
{
    using System;
    using System.Collections.Generic;

    using NSubstitute;
    using Xunit;

    using SolutionDeploy;
    using SolutionDeploy.Core;

    public class DeploymentServiceTest
    {
        DeploymentService sut;

        [Fact]
        public void Ctor_NullCatalogueRepository_ThrowsException()
        {
            IServiceDeploymentHandler deploymentHandler = Substitute.For<IServiceDeploymentHandler>();

            Assert.Throws<ArgumentNullException>(() => this.sut = new DeploymentService(null, deploymentHandler));
        }

        [Fact]
        public void Ctor_NullDeploymentHandler_ThrowsException()
        {
            IProductManifestRepository manifestRepository = Substitute.For<IProductManifestRepository>();

            Assert.Throws<ArgumentNullException>(() => this.sut = new DeploymentService(manifestRepository, null));
        }

        [Fact]
        public void Deploy_NullProduct_ThrowsException()
        {
            IProductManifestRepository manifestRepository = Substitute.For<IProductManifestRepository>();
            IServiceDeploymentHandler deploymentHandler = Substitute.For<IServiceDeploymentHandler>();

            this.sut = new DeploymentService(manifestRepository, deploymentHandler);

            Assert.Throws<ArgumentException>(() => this.sut.Deploy(null, "someVersion", "someEnvironment"));
        }

        [Fact]
        public void Deploy_NullEnvironment_ThrowsException()
        {
            IProductManifestRepository manifestRepository = Substitute.For<IProductManifestRepository>();
            IServiceDeploymentHandler deploymentHandler = Substitute.For<IServiceDeploymentHandler>();

            this.sut = new DeploymentService(manifestRepository, deploymentHandler);

            Assert.Throws<ArgumentException>(() => this.sut.Deploy("someproduct", null, "someVersion"));
        }

        [Fact]
        public void Deploy_NullBranch_UsesDefault()
        {
            IProductManifestRepository manifestRepository = Substitute.For<IProductManifestRepository>();
            IServiceDeploymentHandler deploymentHandler = Substitute.For<IServiceDeploymentHandler>();

            ProductManifest testManifest = GetTestManifest(2);
            manifestRepository.GetManifest(Arg.Any<string>(), Arg.Any<string>())
                .Returns(testManifest);
            this.sut = new DeploymentService(manifestRepository, deploymentHandler);
            this.sut.Deploy("someProduct", "someEnvironment", "someVersion");

            deploymentHandler.DidNotReceive()
                .Deploy(testManifest, "someEnvironment", "someVersion", null);
        }

        [Fact]
        public void Deploy_HasBranch_UsesBranch()
        {
            IProductManifestRepository manifestRepository = Substitute.For<IProductManifestRepository>();
            IServiceDeploymentHandler deploymentHandler = Substitute.For<IServiceDeploymentHandler>();

            ProductManifest testManifest = GetTestManifest(2);
            manifestRepository.GetManifest(Arg.Any<string>(), Arg.Any<string>())
                .Returns(testManifest);
            this.sut = new DeploymentService(manifestRepository, deploymentHandler);
            this.sut.Deploy("someProduct", "someEnvironment", "someVersion", "someBranch");

            deploymentHandler.Received()
                .Deploy(testManifest, "someEnvironment", "someVersion", "someBranch");
        }

        [Fact]
        public void Deploy_NoManifest_DoesNothing()
        {
            IProductManifestRepository manifestRepository = Substitute.For<IProductManifestRepository>();
            IServiceDeploymentHandler deploymentHandler = Substitute.For<IServiceDeploymentHandler>();

            manifestRepository.GetManifest(Arg.Any<string>(), Arg.Any<string>())
                .Returns((ProductManifest)null);

            this.sut = new DeploymentService(manifestRepository, deploymentHandler);
            this.sut.Deploy("someProduct", "someEnvironment", "someVersion");

            deploymentHandler.DidNotReceiveWithAnyArgs()
                .Deploy(Arg.Any<ProductManifest>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void Deploy_GetsManifest_DeploysServices()
        {
            IProductManifestRepository manifestRepository = Substitute.For<IProductManifestRepository>();
            IServiceDeploymentHandler deploymentHandler = Substitute.For<IServiceDeploymentHandler>();

            manifestRepository.GetManifest(Arg.Any<string>(), Arg.Any<string>())
                .Returns(GetTestManifest(2));

            this.sut = new DeploymentService(manifestRepository, deploymentHandler);
            this.sut.Deploy("someProduct", "someEnvironment", "someVersion");

            deploymentHandler.ReceivedWithAnyArgs()
                .Deploy(Arg.Any<ProductManifest>(), Arg.Any<string>(), Arg.Any<string>());
        }

        private ProductManifest GetTestManifest(int count)
        {
            IList<Service> services = new List<Service>();
            for (int i = 0; i < count; i++)
            {
                services.Add(new Service($"service{i}", "someVersion"));
            }

            ProductVersion version = new ProductVersion("someVersion", services);

            ProductManifest catalogue =
                new ProductManifest("someproduct", null, new ProductVersion[] { version });

            return catalogue;
        }
    }
}
