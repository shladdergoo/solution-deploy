namespace SolutionDeploy.Docker
{
    using System;

    using SolutionDeploy.Core;

    public class DockerReleaseRepository : IReleaseRepository
    {
        private readonly string registry;
        private readonly IHttpClient httpClient;

        public DockerReleaseRepository(string registry, IHttpClient httpClient)
        {
            if (string.IsNullOrWhiteSpace(registry)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(registry)); }
            if (httpClient == null) { throw new ArgumentNullException(nameof(httpClient)); }

            this.registry = registry;
            this.httpClient = httpClient;
        }

        public string GetReleaseEnvironmentId(string releaseId, string environmentName)
        {
            throw new NotImplementedException();
        }

        public DeploymentStatus GetReleaseEnvironmentStatus(string releaseId, string environmentId)
        {
            throw new NotImplementedException();
        }

        public string GetReleaseId(string serviceName, string version = null, string branch = null, string prereqEnvironment = null)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(serviceName)); }

            return $"{this.registry}/{serviceName}{(string.IsNullOrWhiteSpace(version) ? null : $":{version}")}";
        }

        public void UpdateApproval(string releaseId)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateReleaseEnvironment(string releaseId, string environmentId)
        {
            throw new System.NotImplementedException();
        }

        public Version GetLatest(string serviceName)
        {
            throw new NotImplementedException();
        }
    }
}
