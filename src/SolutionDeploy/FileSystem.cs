namespace SolutionDeploy
{
    using System.IO;

    internal class FileSystem : IFileSystem
    {
        public Stream OpenRead(string filename)
        {
            return File.OpenRead(filename);
        }

        public Stream OpenWrite(string fileName)
        {
            return File.OpenWrite(fileName);
        }

        public void Delete(string filename)
        {
            File.Delete(filename);
        }
    }
}
