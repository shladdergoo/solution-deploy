namespace SolutionDeploy.Core
{
    public interface IServiceDeploymentExecutor
    {
        bool Deploy(
            string serviceName,
            string environment,
            string version = null,
            string branch = null,
            string prereqEnvironment = null);

        DeploymentStatus GetDeploymentStatus(string serviceName, string environment, string version);
    }
}
