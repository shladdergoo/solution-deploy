namespace SolutionDeploy.Core
{
    using System;
    using System.Collections.Generic;

    public class Manifest
    {
        public Manifest(IEnumerable<ProductManifest> productManifests)
        {
            if (productManifests == null) { throw new ArgumentNullException(nameof(productManifests)); }

            this.Products = productManifests;
        }

        public IEnumerable<ProductManifest> Products { get; }
    }
}
