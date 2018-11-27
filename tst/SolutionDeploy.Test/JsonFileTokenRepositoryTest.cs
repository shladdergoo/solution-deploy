namespace SolutionDeploy.Test
{
    using System;
    using System.IO;

    using Newtonsoft.Json;
    using NSubstitute;
    using Xunit;

    using SolutionDeploy.Core;

    public class JsonFileTokenRepositoryTest
    {
        ITokenRepository sut;

        [Fact]
        public void Ctor_NullFileSystem_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => this.sut = new JsonFileTokenRepository(null));
        }

        [Fact]
        public void GetTokens_FileExists_GetsTokens()
        {
            IFileSystem fileSystem = Substitute.For<IFileSystem>();
            fileSystem.OpenRead(Arg.Any<string>()).Returns(this.GetTestTokenStream());

            this.sut = new JsonFileTokenRepository(fileSystem);

            OAuthAccessTokens result = this.sut.GetTokens();

            Assert.NotNull(result);
        }

        [Fact]
        public void GetTokens_FileDoesntExist_ReturnsNull()
        {
            IFileSystem fileSystem = Substitute.For<IFileSystem>();
            fileSystem.OpenRead(Arg.Any<string>()).Returns((x) => {throw new FileNotFoundException();});

            this.sut = new JsonFileTokenRepository(fileSystem);

            OAuthAccessTokens result = this.sut.GetTokens();

            Assert.Null(result);
        }

        [Fact]
        public void SaveTokens_NullTokens_ThrowsException()
        {
            IFileSystem fileSystem = Substitute.For<IFileSystem>();

            this.sut = new JsonFileTokenRepository(fileSystem);

            Assert.Throws<ArgumentNullException>(() => this.sut.SaveTokens(null));
        }

        [Fact]
        public void SaveTokens_SavesTokens()
        {
            IFileSystem fileSystem = Substitute.For<IFileSystem>();
            fileSystem.OpenWrite(Arg.Any<string>()).Returns(new MemoryStream());
            OAuthAccessTokens accessTokens = new OAuthAccessTokens
            {
                AccessToken = "foo",
                RefreshToken = "bar",
                TokenType = "bundy"
            };

            this.sut = new JsonFileTokenRepository(fileSystem);

            this.sut.SaveTokens(accessTokens);

            fileSystem.ReceivedWithAnyArgs().OpenWrite(default(string));
        }

        private Stream GetTestTokenStream()
        {
            OAuthAccessTokens tokens = new OAuthAccessTokens
            {
                AccessToken = "foo",
                RefreshToken = "bar",
                TokenType = "bundy"
            };

            MemoryStream stream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(stream);

            JsonTextWriter textWriter = new JsonTextWriter(streamWriter);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(textWriter, tokens);
            textWriter.Flush();
            stream.Position = 0;

            return stream;
        }
    }
}
