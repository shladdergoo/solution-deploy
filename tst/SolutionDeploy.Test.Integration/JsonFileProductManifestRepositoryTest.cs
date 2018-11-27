namespace SolutionDeploy.Test.Integration
{
    using System.IO;

    using Xunit;

    using SolutionDeploy.Core;

    public class JsonFileProductManifestRepositoryTest
    {
        IProductManifestRepository sut;

        const string ManifestFilename = "manifest.json";

        [Fact]
        public void GetManifest_WithVersion_Succeeds()
        {
            File.Copy($"../../../testdata/{ManifestFilename}", ManifestFilename, true);

            this.sut = new JsonFileProductManifestRepository(new FileSystem());

            ProductManifest result = this.sut.GetManifest("someProduct", "1.0.0");

            Assert.NotNull(result);

            File.Delete(ManifestFilename);
        }

        [Fact]
        public void GetManifest_NullVersion_Succeeds()
        {
            File.Copy($"../../../testdata/{ManifestFilename}", ManifestFilename, true);

            this.sut = new JsonFileProductManifestRepository(new FileSystem());

            ProductManifest result = this.sut.GetManifest("someProduct", null);

            Assert.NotNull(result);

            File.Delete(ManifestFilename);
        }
    }
}
