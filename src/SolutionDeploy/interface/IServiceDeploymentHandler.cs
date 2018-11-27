namespace SolutionDeploy
{
    using SolutionDeploy.Core;

    internal interface IServiceDeploymentHandler
    {
        void Deploy(
            ProductManifest productManifest,
            string environment,
            string productVersion = null,
            string branch = null);
    }
}
