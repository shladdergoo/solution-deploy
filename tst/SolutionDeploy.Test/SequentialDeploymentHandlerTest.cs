namespace SolutionDeploy.Test
{
    using System;
    using System.Collections.Generic;

    using NSubstitute;
    using Xunit;

    using SolutionDeploy;
    using SolutionDeploy.Core;

    public class SequentialDeploymentHandlerTest
    {
        IServiceDeploymentHandler sut;

        [Fact]
        public void Ctor_NullDeploymentExecutor_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => this.sut = new SequentialDeploymentHandler(null, 0));
        }

        [Fact]
        public void Ctor_NegativeInterval_ThrowsException()
        {
            IServiceDeploymentExecutor deploymentExecutor = Substitute.For<IServiceDeploymentExecutor>();

            Assert.Throws<ArgumentException>(() => this.sut =
                new SequentialDeploymentHandler(deploymentExecutor, -1));
        }

        [Fact]
        public void Deploy_NullService_ThrowsException()
        {
            IServiceDeploymentExecutor deploymentExecutor = Substitute.For<IServiceDeploymentExecutor>();

            this.sut = new SequentialDeploymentHandler(deploymentExecutor, 0);

            Assert.Throws<ArgumentNullException>(() =>
                this.sut.Deploy(null, environment: "someEnvironment", productVersion: "someVersion"));
        }

        [Fact]
        public void Deploy_NullEnvironment_ThrowsException()
        {
            IServiceDeploymentExecutor deploymentExecutor = Substitute.For<IServiceDeploymentExecutor>();

            this.sut = new SequentialDeploymentHandler(deploymentExecutor, 0);

            Assert.Throws<ArgumentException>(() =>
                this.sut.Deploy(GetTestManifest(1), environment: null, productVersion: "someVersion"));
        }

        [Fact]
        public void Deploy_DoesntGetVersion_DoesntDeploy()
        {
            IServiceDeploymentExecutor deploymentExecutor = Substitute.For<IServiceDeploymentExecutor>();

            this.sut = new SequentialDeploymentHandler(deploymentExecutor, 0);
            this.sut.Deploy(GetTestManifest(2), environment: "someEnvironment", productVersion: "wrongVersion");

            deploymentExecutor.DidNotReceiveWithAnyArgs().Deploy(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void Deploy_HasServices_DeploysServices()
        {
            IServiceDeploymentExecutor deploymentExecutor = Substitute.For<IServiceDeploymentExecutor>();
            deploymentExecutor.Deploy(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            deploymentExecutor.GetDeploymentStatus(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(DeploymentStatus.Succeeded);

            this.sut = new SequentialDeploymentHandler(deploymentExecutor, 0);
            this.sut.Deploy(GetTestManifest(2), environment: "someEnvironment", productVersion: "someVersion");

            deploymentExecutor.Received(2).Deploy(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void Deploy_NotStartedSuccessfully_Aborts()
        {
            IServiceDeploymentExecutor deploymentExecutor = Substitute.For<IServiceDeploymentExecutor>();
            deploymentExecutor.Deploy(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(false);
            deploymentExecutor.GetDeploymentStatus(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(DeploymentStatus.Succeeded);

            this.sut = new SequentialDeploymentHandler(deploymentExecutor, 0);
            this.sut.Deploy(GetTestManifest(2), environment: "someEnvironment", productVersion: "someVersion");

            deploymentExecutor.Received(1).Deploy(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void Deploy_DoesntSucceed_Aborts()
        {
            IServiceDeploymentExecutor deploymentExecutor = Substitute.For<IServiceDeploymentExecutor>();
            deploymentExecutor.Deploy(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            deploymentExecutor.GetDeploymentStatus(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(DeploymentStatus.Failed);

            this.sut = new SequentialDeploymentHandler(deploymentExecutor, 0);
            this.sut.Deploy(GetTestManifest(2), environment: "someEnvironment", productVersion: "someVersion");

            deploymentExecutor.Received(1).Deploy(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void Deploy_Whatif_DoesntWait()
        {
            IServiceDeploymentExecutor deploymentExecutor = Substitute.For<IServiceDeploymentExecutor>();
            deploymentExecutor.Deploy(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            Stack<DeploymentStatus> statusResults = new Stack<DeploymentStatus>();
            statusResults.Push(DeploymentStatus.Succeeded);
            statusResults.Push(DeploymentStatus.InProgress);

            deploymentExecutor.GetDeploymentStatus(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x => statusResults.Pop());

            this.sut = new SequentialDeploymentHandler(deploymentExecutor, 0, new Options { WhatIf = true });
            this.sut.Deploy(GetTestManifest(1), environment: "someEnvironment", productVersion: "someVersion");

            deploymentExecutor.DidNotReceiveWithAnyArgs().GetDeploymentStatus(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void Deploy_DeploymentInProgess_Waits()
        {
            IServiceDeploymentExecutor deploymentExecutor = Substitute.For<IServiceDeploymentExecutor>();
            deploymentExecutor.Deploy(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            Stack<DeploymentStatus> statusResults = new Stack<DeploymentStatus>();
            statusResults.Push(DeploymentStatus.Succeeded);
            statusResults.Push(DeploymentStatus.InProgress);

            deploymentExecutor.GetDeploymentStatus(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x => statusResults.Pop());

            this.sut = new SequentialDeploymentHandler(deploymentExecutor, 0);
            this.sut.Deploy(GetTestManifest(1), environment: "someEnvironment", productVersion: "someVersion");

            deploymentExecutor.Received(2).GetDeploymentStatus(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void Deploy_DeploymentQueued_Waits()
        {
            IServiceDeploymentExecutor deploymentExecutor = Substitute.For<IServiceDeploymentExecutor>();
            deploymentExecutor.Deploy(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            Stack<DeploymentStatus> statusResults = new Stack<DeploymentStatus>();
            statusResults.Push(DeploymentStatus.Succeeded);
            statusResults.Push(DeploymentStatus.Queued);

            deploymentExecutor.GetDeploymentStatus(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(x => statusResults.Pop());

            this.sut = new SequentialDeploymentHandler(deploymentExecutor, 0);
            this.sut.Deploy(GetTestManifest(1), environment: "someEnvironment", productVersion: "someVersion");

            deploymentExecutor.Received(2).GetDeploymentStatus(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        private ProductManifest GetTestManifest(int count)
        {
            IList<Service> services = new List<Service>();
            for (int i = 0; i < count; i++)
            {
                services.Add(new Service($"service{i}", "someVersion"));
            }

            ProductManifest manifest =
                new ProductManifest(
                    "someproduct",
                    null,
                    new ProductVersion[] { new ProductVersion("someVersion", services) });

            return manifest;
        }
    }
}
