namespace SolutionDeploy.Test.Integration
{
    using System.IO;

    using Xunit;

    using SolutionDeploy.Core;

    public class JsonFileTokenRepositoryTest
    {
        ITokenRepository sut;

        const string TokenFilename = "solutiondeploy_tokens.json";

        [Fact]
        public void GetTokens_Succeeds()
        {
            File.Copy($"../../../testdata/{TokenFilename}", TokenFilename, true);

            this.sut = new JsonFileTokenRepository(new FileSystem());

            OAuthAccessTokens result = this.sut.GetTokens();

            Assert.NotNull(result);

            File.Delete(TokenFilename);
        }

        [Fact]
        public void SaveTokens_Succeeds()
        {
            OAuthAccessTokens accessTokens = new OAuthAccessTokens
            {
                AccessToken = "foo",
                RefreshToken = "bar",
                TokenType = "bundy"
            };
            File.Delete(TokenFilename);

            this.sut = new JsonFileTokenRepository(new FileSystem());

            this.sut.SaveTokens(accessTokens);

            Assert.True(File.Exists(TokenFilename));

            File.Delete(TokenFilename);
        }
    }
}
