namespace SolutionDeploy
{
    using System;
    using System.Linq;
    using System.Threading;

    using Microsoft.Extensions.Logging;

    using SolutionDeploy.Core;

    internal class SequentialDeploymentHandler : IServiceDeploymentHandler
    {
        private readonly IServiceDeploymentExecutor deploymentExecutor;
        private readonly int deploymentStatusCheckInterval;
        private readonly Options options;
        private ILogger logger = Logging.GetLogger<SequentialDeploymentHandler>();

        public SequentialDeploymentHandler(
            IServiceDeploymentExecutor deploymentExecutor,
            int deploymentStatusCheckInterval,
            Options options = null)
        {
            if (deploymentStatusCheckInterval < 0) { throw new ArgumentException("parameter cannot be less than 0", nameof(deploymentStatusCheckInterval)); }

            this.deploymentExecutor = deploymentExecutor ?? throw new ArgumentNullException(nameof(deploymentExecutor));
            this.deploymentStatusCheckInterval = deploymentStatusCheckInterval * 1000;
            this.options = options;
        }

        public void Deploy(
            ProductManifest productManifest,
            string environment,
            string version = null,
            string branch = null)
        {
            if (productManifest == null) { throw new ArgumentNullException(nameof(productManifest)); }
            if (string.IsNullOrWhiteSpace(environment)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(environment)); }

            ProductVersion productVersion = productManifest.Versions.FirstOrDefault(
                v => v.Version == version);
            if (productVersion == null) { return; }

            foreach (Service service in productVersion.Services)
            {
                if (this.StartDeployment(
                    service,
                    environment,
                    branch,
                    productManifest.PrereqEnvironment))
                {
                    if (this.options == null || !this.options.WhatIf)
                    {
                        DeploymentStatus status =
                            this.WaitForDeploymentComplete(service, environment);

                        if (status != DeploymentStatus.Succeeded) { break; }
                    }
                }
                else { break; }
            }
        }

        private bool StartDeployment(
            Service service,
            string targetEnvironment,
            string branch,
            string prereqEnvironment)
        {
            this.logger.LogInformation($"starting deployment: [{service.Name}], [{targetEnvironment}], [{service.Version}], [{prereqEnvironment}]");

            return this.deploymentExecutor.Deploy(
                service.Name,
                targetEnvironment,
                version: service.Version,
                branch: branch,
                prereqEnvironment: prereqEnvironment);
        }

        private DeploymentStatus WaitForDeploymentComplete(Service service, string environment)
        {
            int count = 0;
            DeploymentStatus status;
            do
            {
                count++;
                status =
                    this.deploymentExecutor.GetDeploymentStatus(service.Name, environment, service.Version);
                this.logger.LogInformation($"deployment status: {count.ToString().PadLeft(2)} [{status}]");

                if ((status != DeploymentStatus.Queued) && (status != DeploymentStatus.InProgress)
                    && (status != DeploymentStatus.PendingApproval))
                {
                    continue;
                }

                Thread.Sleep(this.deploymentStatusCheckInterval);
            }
            while ((status == DeploymentStatus.Queued) || (status == DeploymentStatus.InProgress)
                || (status == DeploymentStatus.PendingApproval));

            return status;
        }
    }
}
