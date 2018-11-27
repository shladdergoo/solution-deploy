namespace SolutionDeploy
{
    using System.IO;

    public interface IFileSystem
    {
        Stream OpenRead(string filename);

        Stream OpenWrite(string fileName);

        void Delete(string fileName);
    }
}
