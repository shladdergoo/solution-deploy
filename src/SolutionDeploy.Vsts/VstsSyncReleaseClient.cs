namespace SolutionDeploy.Vsts
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Extensions.Logging;

    using Microsoft.TeamFoundation.Build.WebApi;
    using Microsoft.VisualStudio.Services.OAuth;
    using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
    using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
    using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
    using Microsoft.VisualStudio.Services.WebApi;

    using SolutionDeploy.Core;

    public class VstsSyncReleaseClient : IVstsReleaseClient
    {
        private const int EnvironmentStatusSucceeded = 4;
        private const int EnvironmentStatusPartiallySucceeded = 128;
        private const int BuildsReturnTop = 1;
        private const string DeployMessage = "Deployed using solution deploy tool";
        private const string ApproveMessage = "Approved using solution deploy tool";

        private readonly VstsConfig vstsConfig;
        private readonly Options options;
        private ILogger logger = Logging.GetLogger<VstsSyncReleaseClient>();

        public VstsSyncReleaseClient(VstsConfig vstsConfig, Options options = null)
        {
            if (vstsConfig == null) { throw new ArgumentNullException(nameof(vstsConfig)); }

            this.vstsConfig = vstsConfig;
            this.options = options;
        }

        public IEnumerable<ReleaseDefinition> GetReleaseDefinitions(
            string definitionName,
            string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(accessToken)); }

            this.logger.LogDebug($"getting release definitions. definition name:[{definitionName}]");

            ReleaseHttpClient client = this.GetClient<ReleaseHttpClient>(accessToken);

            return client.GetReleaseDefinitionsAsync(
                this.vstsConfig.ProjectName,
                searchText: definitionName,
                expand: ReleaseDefinitionExpands.Artifacts | ReleaseDefinitionExpands.Environments).Result;
        }

        public ReleaseDefinition GetReleaseDefinition(int id, string accessToken)
        {
            if (id < 0) { throw new ArgumentException("id cannot be less than 0", nameof(id)); }
            if (string.IsNullOrWhiteSpace(accessToken)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(accessToken)); }

            this.logger.LogDebug($"getting release definition. id:[{id}]");

            ReleaseHttpClient client = this.GetClient<ReleaseHttpClient>(accessToken);

            return client.GetReleaseDefinitionAsync(this.vstsConfig.ProjectName, id).Result;
        }

        public IEnumerable<Release> GetReleases(
            int definitionId,
            string accessToken,
            int? artifactVersionId = null,
            int? prereqEnvId = null)
        {
            if (definitionId < 0) { throw new ArgumentException("id cannot be less than 0", nameof(definitionId)); }
            if (string.IsNullOrWhiteSpace(accessToken)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(accessToken)); }

            this.logger.LogDebug($"getting releases. definitionId:[{definitionId}], artifaction version id:[{artifactVersionId}], prereq environment id:[{prereqEnvId}]");

            ReleaseHttpClient client = this.GetClient<ReleaseHttpClient>(accessToken);

            bool hasArtifactVersionId = artifactVersionId.HasValue && artifactVersionId > 0;
            bool hasPrereqEnvId = prereqEnvId.HasValue && prereqEnvId > 0;
            int? environmentStatusFilter = this.GetEnvironmentStatusFilter();

            return client.GetReleasesAsync(
                this.vstsConfig.ProjectName,
                definitionId,
                statusFilter: ReleaseStatus.Active,
                artifactVersionId: hasArtifactVersionId ? artifactVersionId.ToString() : null,
                definitionEnvironmentId: prereqEnvId,
                environmentStatusFilter: hasArtifactVersionId ? environmentStatusFilter : (int?)null,
                expand: ReleaseExpands.Artifacts).Result;
        }

        public Release GetRelease(int id, string accessToken)
        {
            if (id < 0) { throw new ArgumentException("id cannot be less than 0", nameof(id)); }
            if (string.IsNullOrWhiteSpace(accessToken)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(accessToken)); }

            this.logger.LogDebug($"getting release. id:[{id}]");

            ReleaseHttpClient client = this.GetClient<ReleaseHttpClient>(accessToken);

            return client.GetReleaseAsync(this.vstsConfig.ProjectName, id).Result;
        }

        public IEnumerable<Build> GetBuilds(
            int definitionId,
            string accessToken,
            string buildNumber = null,
            string branch = null)
        {
            if (definitionId < 0) { throw new ArgumentException("id cannot be less than 0", nameof(definitionId)); }
            if (string.IsNullOrWhiteSpace(accessToken)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(accessToken)); }

            this.logger.LogDebug($"getting builds. definition id:[{definitionId}], build number:[{buildNumber}], branch:[{branch}]");

            BuildHttpClient client = this.GetClient<BuildHttpClient>(accessToken);

            return client.GetBuildsAsync(
                this.vstsConfig.ProjectName,
                new[] { definitionId },
                buildNumber: buildNumber,
                branchName: branch,
                top: branch == null ? (int?)null : BuildsReturnTop).Result;
        }

        public ReleaseEnvironment UpdateReleaseEnvironment(int releaseId, int targetEnvId, string accessToken)
        {
            if (releaseId < 0) { throw new ArgumentException("id cannot be less than 0", nameof(releaseId)); }
            if (targetEnvId < 0) { throw new ArgumentException("id cannot be less than 0", nameof(targetEnvId)); }
            if (string.IsNullOrWhiteSpace(accessToken)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(accessToken)); }

            this.logger.LogDebug($"updating release environment. release id:[{releaseId}], target environment id:[{targetEnvId}]");

            ReleaseHttpClient client = this.GetClient<ReleaseHttpClient>(accessToken);

            ReleaseEnvironmentUpdateMetadata environmentMetadata = new ReleaseEnvironmentUpdateMetadata()
            {
                Status = EnvironmentStatus.InProgress,
                Comment = DeployMessage
            };

            return client.UpdateReleaseEnvironmentAsync(
                environmentMetadata,
                this.vstsConfig.ProjectName,
                releaseId,
                targetEnvId).Result;
        }

        public IEnumerable<ReleaseApproval> GetApprovals(int releaseId, string accessToken)
        {
            if (releaseId < 0) { throw new ArgumentException("id cannot be less than 0", nameof(releaseId)); }
            if (string.IsNullOrWhiteSpace(accessToken)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(accessToken)); }

            this.logger.LogDebug($"getting approvals. release id:[{releaseId}]");

            ReleaseHttpClient client = this.GetClient<ReleaseHttpClient>(accessToken);

            return client.GetApprovalsAsync(
                this.vstsConfig.ProjectName,
                releaseIdsFilter: new int[] { releaseId }).Result;
        }

        public ReleaseApproval UpdateApproval(int approvalId, string accessToken)
        {
            if (approvalId < 0) { throw new ArgumentException("id cannot be less than 0", nameof(approvalId)); }
            if (string.IsNullOrWhiteSpace(accessToken)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(accessToken)); }

            this.logger.LogDebug($"updating approval. approval id:[{approvalId}]");

            ReleaseApproval approval = new ReleaseApproval
            {
                Status = ApprovalStatus.Approved,
                Comments = ApproveMessage
            };

            ReleaseHttpClient client = this.GetClient<ReleaseHttpClient>(accessToken);

            return client.UpdateReleaseApprovalAsync(
                approval,
                this.vstsConfig.ProjectName,
                approvalId).Result;
        }

        private int? GetEnvironmentStatusFilter()
        {
            if (this.options != null && this.options.AllowPartial)
            {
                return EnvironmentStatusSucceeded | EnvironmentStatusPartiallySucceeded;
            }
            else
            {
                return EnvironmentStatusSucceeded;
            }
        }

        private T GetClient<T>(string accessToken)
            where T : VssHttpClientBase
        {
            VssConnection vssConnection = new VssConnection(
                new Uri(this.vstsConfig.BaseUrl),
                new VssOAuthAccessTokenCredential(accessToken));

            return vssConnection.GetClient<T>();
        }
    }
}
