namespace SolutionDeploy.Vsts
{
    using System;

    using Microsoft.Extensions.Logging;

    using SolutionDeploy.Core;

    public class VstsDeploymentExecutor : IServiceDeploymentExecutor
    {
        private readonly IReleaseRepository releaseRepository;
        private readonly VstsConfig vstsConfig;
        private readonly Options options;
        private ILogger logger = Logging.GetLogger<VstsDeploymentExecutor>();

        public VstsDeploymentExecutor(
            IReleaseRepository releaseRepository,
            VstsConfig vstsConfig,
            Options options = null)
        {
            if (releaseRepository == null) { throw new ArgumentNullException(nameof(releaseRepository)); }
            if (vstsConfig == null) { throw new ArgumentNullException(nameof(vstsConfig)); }

            this.releaseRepository = releaseRepository;
            this.vstsConfig = vstsConfig;
            this.options = options;
        }

        public bool Deploy(
            string serviceName,
            string environment,
            string version = null,
            string branch = null,
            string prereqEnvironment = null)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(serviceName)); }
            if (string.IsNullOrWhiteSpace(environment)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(environment)); }

            string releaseId = this.releaseRepository.GetReleaseId(
                serviceName,
                version: version,
                branch: branch,
                prereqEnvironment: prereqEnvironment);
            if (releaseId == null)
            {
                this.logger.LogWarning("could not get release id");
                return false;
            }

            this.logger.LogDebug($"got releaseId:[{releaseId}]");

            string environmentId = this.releaseRepository.GetReleaseEnvironmentId(
                releaseId, environment);
            if (environmentId == null)
            {
                this.logger.LogWarning("could not get release environment id");
                return false;
            }

            if (this.options == null || !this.options.WhatIf)
            {
                this.releaseRepository.UpdateReleaseEnvironment(releaseId, environmentId);
            }

            return true;
        }

        public DeploymentStatus GetDeploymentStatus(string serviceName, string environment, string version)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(serviceName)); }
            if (string.IsNullOrWhiteSpace(environment)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(environment)); }

            string releaseId = this.releaseRepository.GetReleaseId(serviceName, version: version);
            if (releaseId == null) { return DeploymentStatus.Unknown; }

            string environmentId = this.releaseRepository.GetReleaseEnvironmentId(
                releaseId, environment);
            if (environmentId == null) { return DeploymentStatus.Unknown; }

            DeploymentStatus status = this.releaseRepository
                .GetReleaseEnvironmentStatus(releaseId, environmentId);

            if (status == DeploymentStatus.PendingApproval)
            {
                this.ApproveRelease(releaseId);
            }

            return status;
        }

        private void ApproveRelease(string releaseId)
        {
            this.logger.LogInformation("approval required. attempting to approve release");

            this.releaseRepository.UpdateApproval(releaseId);
        }
    }
}
