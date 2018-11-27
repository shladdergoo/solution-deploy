namespace SolutionDeploy.Core
{
    using System;
    using System.Collections.Generic;

    public class ProductManifest
    {
        public ProductManifest(string name, string prereqEnvironment, IEnumerable<ProductVersion> versions)
        {
            if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentException("parameter cannot be null or whitespace", nameof(name)); }
            if (versions == null) { throw new ArgumentNullException(nameof(versions)); }

            this.Name = name;
            this.PrereqEnvironment = prereqEnvironment;
            this.Versions = versions;
        }

        public string Name { get; }

        public string PrereqEnvironment { get; }

        public IEnumerable<ProductVersion> Versions { get; }
    }
}
