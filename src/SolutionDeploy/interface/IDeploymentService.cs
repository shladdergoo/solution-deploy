namespace SolutionDeploy
{
    internal interface IDeploymentService
    {
        void Deploy(
            string productName,
            string environment,
            string productVersion = null,
            string branch = null);
    }
}
