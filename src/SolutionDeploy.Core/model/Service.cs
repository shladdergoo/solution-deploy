namespace SolutionDeploy.Core
{
    using System;

    public class Service
    {
        private string name;
        private string serviceName;
        private string version;

        public Service(string name, string serviceName = null, string version = null)
        {
            if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(name)); }

            this.Name = name;
            this.ServiceName = serviceName;
            this.Version = version;
        }

        public string Name { get => this.name; private set => this.name = value; }

        public string ServiceName { get => this.serviceName; private set => this.serviceName = value; }

        public string Version { get => this.version; private set => this.version = value; }
    }
}
