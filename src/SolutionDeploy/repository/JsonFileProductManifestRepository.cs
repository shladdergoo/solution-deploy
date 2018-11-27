namespace SolutionDeploy
{
    using System;
    using System.IO;
    using System.Linq;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    using SolutionDeploy.Core;

    internal class JsonFileProductManifestRepository : IProductManifestRepository
    {
        private const string ManifestFileName = @"manifest.json";

        private readonly IFileSystem fileSystem;

        private ILogger logger = Logging.GetLogger<JsonFileProductManifestRepository>();

        public JsonFileProductManifestRepository(IFileSystem fileSystem)
        {
            if (fileSystem == null) { throw new System.ArgumentNullException(nameof(fileSystem)); }

            this.fileSystem = fileSystem;
        }

        public ProductManifest GetManifest(string product, string version)
        {
            if (string.IsNullOrWhiteSpace(product)) { throw new System.ArgumentException("parameter cannot be null or whitespace", nameof(product)); }

            this.logger.LogDebug($"reading from manifest file:[{Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + ManifestFileName}]");

            StreamReader streamReader = new StreamReader(this.fileSystem.OpenRead(ManifestFileName));

            Manifest manifest = null;
            using (JsonTextReader textReader = new JsonTextReader(streamReader))
            {
                JsonSerializer serializer = new JsonSerializer();
                manifest = serializer.Deserialize<Manifest>(textReader);
            }

            return GetProductManifest(manifest, product, version);
        }

        private static ProductManifest GetProductManifest(
            Manifest manifest, string product, string version)
        {
            ProductManifest productManifest = manifest.Products.FirstOrDefault(
                p => p.Name.Equals(product, StringComparison.InvariantCultureIgnoreCase));

            if (productManifest == null) { return null; }

            if (!productManifest.Versions.Any(v => v.Version == version)) { return null; }

            return new ProductManifest(
                productManifest.Name,
                productManifest.PrereqEnvironment,
                productManifest.Versions.Where(v => v.Version == version));
        }
    }
}
