namespace SolutionDeploy
{
    using System;
    using System.IO;

    using Newtonsoft.Json;

    using SolutionDeploy.Core;

    internal class JsonFileTokenRepository : ITokenRepository
    {
        private const string TokenFilename = @"solutiondeploy_tokens.json";

        private readonly IFileSystem fileSystem;

        public JsonFileTokenRepository(IFileSystem fileSystem)
        {
            if (fileSystem == null) { throw new ArgumentNullException(nameof(fileSystem)); }

            this.fileSystem = fileSystem;
        }

        public OAuthAccessTokens GetTokens()
        {
            OAuthAccessTokens tokens = null;

            try
            {
                string fullTokenFilename =
                    Path.Combine(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"), TokenFilename);
                StreamReader streamReader = new StreamReader(this.fileSystem.OpenRead(fullTokenFilename));

                using (JsonTextReader textReader = new JsonTextReader(streamReader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    tokens = serializer.Deserialize<OAuthAccessTokens>(textReader);
                }
            }
            catch (FileNotFoundException) { }

            return tokens;
        }

        public void SaveTokens(OAuthAccessTokens accessTokens)
        {
            if (accessTokens == null) { throw new ArgumentNullException(nameof(accessTokens)); }

            string fullTokenFilename =
                Path.Combine(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"), TokenFilename);

            this.fileSystem.Delete(fullTokenFilename);

            StreamWriter streamWriter = new StreamWriter(this.fileSystem.OpenWrite(fullTokenFilename));

            using (JsonTextWriter textWriter = new JsonTextWriter(streamWriter))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(textWriter, accessTokens);
                textWriter.Flush();
            }
        }
    }
}
