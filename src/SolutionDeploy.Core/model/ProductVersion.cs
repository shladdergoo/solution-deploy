namespace SolutionDeploy.Core
{
    using System.Collections.Generic;

    public class ProductVersion
    {
        public ProductVersion(string version, IEnumerable<Service> services)
        {
            if (services == null) { throw new System.ArgumentNullException(nameof(services)); }

            this.Version = version;
            this.Services = services;
        }

        public string Version { get; }

        public IEnumerable<Service> Services { get; }
    }
}
