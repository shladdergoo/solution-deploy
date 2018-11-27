namespace SolutionDeploy.Vsts
{
    using System.Collections.Generic;

    using Microsoft.TeamFoundation.Build.WebApi;
    using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;

    public interface IVstsReleaseClient
    {
        IEnumerable<ReleaseDefinition> GetReleaseDefinitions(string defintionName, string accessToken);

        ReleaseDefinition GetReleaseDefinition(int id, string accessToken);

        IEnumerable<Release> GetReleases(
            int definitionId,
            string accessToken,
            int? artifactVersionId = null,
            int? prereqEnvId = null);

        Release GetRelease(int id, string accessToken);

        IEnumerable<Build> GetBuilds(
            int definitionId,
            string accessToken,
            string buildNumber = null,
            string branch = null);

        ReleaseEnvironment UpdateReleaseEnvironment(int releaseId, int targetEnvId, string accessToken);

        IEnumerable<ReleaseApproval> GetApprovals(int releaseId, string accessToken);

        ReleaseApproval UpdateApproval(int approvalId, string accessToken);
    }
}
