namespace SolutionDeploy.Core
{
    using System;

    public interface IReleaseRepository
    {
        string GetReleaseId(
            string serviceName,
            string version = null,
            string branch = null,
            string prereqEnvironment = null);

        string GetReleaseEnvironmentId(string releaseId, string environmentName);

        void UpdateReleaseEnvironment(string releaseId, string environmentId);

        DeploymentStatus GetReleaseEnvironmentStatus(string releaseId, string environmentId);

        void UpdateApproval(string releaseId);

        Version GetLatest(string serviceName);
    }
}
