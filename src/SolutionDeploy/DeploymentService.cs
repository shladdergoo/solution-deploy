namespace SolutionDeploy
{
    using System;

    using Microsoft.Extensions.Logging;

    using SolutionDeploy.Core;

    internal class DeploymentService : IDeploymentService
    {
        private const string DefaultBranch = "/ref/heads/master";
        private readonly IProductManifestRepository manifestRepository;
        private readonly IServiceDeploymentHandler deploymentHandler;
        private ILogger logger = Logging.GetLogger<DeploymentService>();

        public DeploymentService(
            IProductManifestRepository manifestRepository, IServiceDeploymentHandler deploymentHandler)
        {
            this.manifestRepository = manifestRepository ?? throw new ArgumentNullException(nameof(manifestRepository));
            this.deploymentHandler = deploymentHandler ?? throw new ArgumentNullException(nameof(deploymentHandler));
        }

        public void Deploy(
            string productName,
            string environment,
            string productVersion = null,
            string branch = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productName)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(productName)); }
                if (string.IsNullOrWhiteSpace(environment)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(environment)); }
                if (branch == null) { branch = DefaultBranch; }

                this.logger.LogDebug("starting");

                ProductManifest productManifest = this.manifestRepository.GetManifest(productName, productVersion);

                if (productManifest == null)
                {
                    this.logger.LogWarning($"could not find product:[{productName}] with version:[{productVersion}] in the manifest");
                    return;
                }

                this.deploymentHandler.Deploy(
                    productManifest,
                    environment,
                    productVersion: productVersion,
                    branch: branch);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "application exception");
                throw;
            }
        }
    }
}
