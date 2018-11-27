namespace SolutionDeploy
{
    using SolutionDeploy.Core;

    internal interface IProductManifestRepository
    {
        ProductManifest GetManifest(string product, string version);
    }
}
