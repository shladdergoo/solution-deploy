namespace SolutionDeploy.Vsts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Authentication;

    using Microsoft.TeamFoundation.Build.WebApi;
    using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
    using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;

    using SolutionDeploy.Core;

    public class VstsReleaseRepository : IReleaseRepository
    {
        private readonly IVstsReleaseClient releaseClient;
        private readonly IAuthenticator authenticator;
        private readonly VstsConfig vstsConfig;

        private string accessToken;
        private bool releaseApproved;

        public VstsReleaseRepository(
            IVstsReleaseClient releaseClient,
            IAuthenticator authenticator,
            VstsConfig vstsConfig)
        {
            if (releaseClient == null) { throw new ArgumentNullException(nameof(releaseClient)); }
            if (authenticator == null) { throw new ArgumentNullException(nameof(authenticator)); }
            if (vstsConfig == null) { throw new ArgumentNullException(nameof(vstsConfig)); }

            this.releaseClient = releaseClient;
            this.authenticator = authenticator;
            this.vstsConfig = vstsConfig;
        }

        public string GetReleaseEnvironmentId(string releaseId, string environmentName)
        {
            if (string.IsNullOrWhiteSpace(releaseId)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(releaseId)); }
            if (string.IsNullOrWhiteSpace(environmentName)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(environmentName)); }

            int releaseIdInt;
            if (!int.TryParse(releaseId, out releaseIdInt)) { throw new ArgumentException("could not convert to Int32", nameof(releaseId)); }

            AuthenticationResult authenticationResult = this.authenticator.Authenticate();

            if (!authenticationResult.Success)
            {
                throw new AuthenticationException("could not authenticate", authenticationResult.Exception);
            }

            this.accessToken = authenticationResult.AccessToken;
            return this.DoGetReleaseEnvironmentId(releaseIdInt, environmentName);
        }

        public string GetReleaseId(
            string serviceName,
            string version = null,
            string branch = null,
            string prereqEnvironment = null)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(serviceName)); }

            AuthenticationResult authenticationResult = this.authenticator.Authenticate();

            if (!authenticationResult.Success)
            {
                throw new AuthenticationException("could not authenticate", authenticationResult.Exception);
            }

            this.accessToken = authenticationResult.AccessToken;
            return this.DoGetReleaseId(serviceName, version, branch, prereqEnvironment);
        }

        public void UpdateReleaseEnvironment(string releaseId, string environmentId)
        {
            if (string.IsNullOrWhiteSpace(releaseId)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(releaseId)); }
            if (string.IsNullOrWhiteSpace(environmentId)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(environmentId)); }

            int releaseIdInt;
            if (!int.TryParse(releaseId, out releaseIdInt)) { throw new ArgumentException("could not convert to Int32", nameof(releaseId)); }
            int environmentIdInt;
            if (!int.TryParse(environmentId, out environmentIdInt)) { throw new ArgumentException("could not convert to Int32", nameof(environmentId)); }

            AuthenticationResult authenticationResult = this.authenticator.Authenticate();

            if (!authenticationResult.Success)
            {
                throw new AuthenticationException("could not authenticate", authenticationResult.Exception);
            }

            this.accessToken = authenticationResult.AccessToken;

            this.releaseClient.UpdateReleaseEnvironment(
                releaseIdInt,
                environmentIdInt,
                this.accessToken);
        }

        public SolutionDeploy.Core.DeploymentStatus GetReleaseEnvironmentStatus(string releaseId, string environmentId)
        {
            if (string.IsNullOrWhiteSpace(releaseId)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(releaseId)); }
            if (string.IsNullOrWhiteSpace(environmentId)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(environmentId)); }

            int releaseIdInt;
            if (!int.TryParse(releaseId, out releaseIdInt)) { throw new ArgumentException("could not convert to Int32", nameof(releaseId)); }
            int environmentIdInt;
            if (!int.TryParse(environmentId, out environmentIdInt)) { throw new ArgumentException("could not convert to Int32", nameof(environmentId)); }

            AuthenticationResult authenticationResult = this.authenticator.Authenticate();

            if (!authenticationResult.Success)
            {
                throw new AuthenticationException("could not authenticate", authenticationResult.Exception);
            }

            return this.DoGetReleaseEnvironmentStatus(releaseIdInt, environmentIdInt);
        }

        public void UpdateApproval(string releaseId)
        {
            if (string.IsNullOrWhiteSpace(releaseId)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(releaseId)); }

            int releaseIdInt;
            if (!int.TryParse(releaseId, out releaseIdInt)) { throw new ArgumentException("could not convert to Int32", nameof(releaseId)); }

            AuthenticationResult authenticationResult = this.authenticator.Authenticate();

            if (!authenticationResult.Success)
            {
                throw new AuthenticationException("could not authenticate", authenticationResult.Exception);
            }

            this.DoUpdateApproval(releaseIdInt);
        }

        public Version GetLatest(string serviceName)
        {
            throw new NotImplementedException();
        }

        private static SolutionDeploy.Core.DeploymentStatus GetDeploymentStatus(EnvironmentStatus status)
        {
            switch (status)
            {
                case EnvironmentStatus.Canceled: return SolutionDeploy.Core.DeploymentStatus.Cancelled;
                case EnvironmentStatus.NotStarted: return SolutionDeploy.Core.DeploymentStatus.NotStarted;
                case EnvironmentStatus.PartiallySucceeded: return SolutionDeploy.Core.DeploymentStatus.Failed;
                case EnvironmentStatus.Queued: return SolutionDeploy.Core.DeploymentStatus.Queued;
                case EnvironmentStatus.Rejected: return SolutionDeploy.Core.DeploymentStatus.Failed;
                case EnvironmentStatus.Scheduled: return SolutionDeploy.Core.DeploymentStatus.Queued;
                case EnvironmentStatus.Succeeded: return SolutionDeploy.Core.DeploymentStatus.Succeeded;
                case EnvironmentStatus.Undefined: return SolutionDeploy.Core.DeploymentStatus.Unknown;
                default: return SolutionDeploy.Core.DeploymentStatus.Unknown;
            }
        }

        private static int? GetDefintionEnvId(string prereqEnv, ReleaseDefinition releaseDefintion)
        {
            int? defintionEnvId = null;
            if (prereqEnv != null)
            {
                defintionEnvId = releaseDefintion.Environments?.FirstOrDefault(
                    e => e.Name.Equals(prereqEnv, StringComparison.InvariantCultureIgnoreCase))?
                        .Id;
            }

            return defintionEnvId;
        }

        private string DoGetReleaseEnvironmentId(
            int releaseId, string environmentName)
        {
            Release release = this.releaseClient.GetRelease(releaseId, this.accessToken);

            if (release == null) { return null; }

            ReleaseEnvironment environment = release.Environments
                .FirstOrDefault(e => e.Name.Equals(environmentName, StringComparison.InvariantCultureIgnoreCase));

            if (environment == null) { return null; }

            return environment.Id.ToString();
        }

        private string DoGetReleaseId(
            string serviceName, string version, string branch, string prereqEnv)
        {
            ReleaseDefinition releaseDefintion =
                this.releaseClient.GetReleaseDefinitions(serviceName, this.accessToken)
                    .FirstOrDefault();

            if (releaseDefintion == null) { return null; }

            int? defintionEnvId = GetDefintionEnvId(prereqEnv, releaseDefintion);

            int? versionBuildId =
                this.GetVersionBuildId(version, branch, releaseDefintion);

            IEnumerable<Release> releases = this.releaseClient
                .GetReleases(releaseDefintion.Id, this.accessToken, versionBuildId, defintionEnvId);

            if (!releases.Any()) { return null; }

            return releases.First().Id.ToString();
        }

        private SolutionDeploy.Core.DeploymentStatus DoGetReleaseEnvironmentStatus(int releaseId, int environmentId)
        {
            Release release = this.releaseClient.GetRelease(releaseId, this.accessToken);

            if (release == null) { return SolutionDeploy.Core.DeploymentStatus.Unknown; }

            ReleaseEnvironment releaseEnvironment =
                release.Environments.FirstOrDefault(e => e.Id == environmentId);

            if (releaseEnvironment == null) { return SolutionDeploy.Core.DeploymentStatus.Unknown; }

            EnvironmentStatus environmentStatus = releaseEnvironment.Status;

            if (environmentStatus == EnvironmentStatus.InProgress)
            {
                return this.CheckForPendingApprovals(releaseId);
            }

            return GetDeploymentStatus(environmentStatus);
        }

        private void DoUpdateApproval(int releaseId)
        {
            IEnumerable<ReleaseApproval> approvals =
                this.releaseClient.GetApprovals(releaseId, this.accessToken);

            if (!approvals.Any()) { return; }

            this.releaseClient.UpdateApproval(approvals.First().Id, this.accessToken);
        }

        private int? GetVersionBuildId(
            string version,
            string branch,
            ReleaseDefinition releaseDefintion)
        {
            int? versionBuildId = null;
            if (version != null || branch != null)
            {
                IEnumerable<Build> versionBuilds =
                    this.GetVersionBuilds(releaseDefintion, version, branch);

                versionBuildId = versionBuilds.FirstOrDefault()?.Id;
            }

            return versionBuildId;
        }

        private IEnumerable<Build> GetVersionBuilds(
            ReleaseDefinition releaseDefintion,
            string version,
            string branch)
        {
            Artifact primaryArtifact = releaseDefintion.Artifacts
                .First(a => a.IsPrimary == true);

            int artifactBuildDefinitionId =
                Convert.ToInt32(primaryArtifact.DefinitionReference["definition"].Id);

            return this.releaseClient
                .GetBuilds(artifactBuildDefinitionId, this.accessToken, version, branch);
        }

        private SolutionDeploy.Core.DeploymentStatus CheckForPendingApprovals(int releaseId)
        {
            if (this.releaseApproved) { return SolutionDeploy.Core.DeploymentStatus.InProgress; }

            IEnumerable<ReleaseApproval> approvals =
                this.releaseClient.GetApprovals(releaseId, this.accessToken);

            if (!approvals.Any())
            {
                this.releaseApproved = true;
                return SolutionDeploy.Core.DeploymentStatus.InProgress;
            }

            return SolutionDeploy.Core.DeploymentStatus.PendingApproval;
        }
    }
}
