namespace SolutionDeploy.Test
{
    using System;
    using System.IO;
    using System.Text;

    using Newtonsoft.Json;
    using NSubstitute;
    using Xunit;

    using SolutionDeploy.Core;

    public class JsonFileProductManifestRepositoryTest
    {
        JsonFileProductManifestRepository sut;

        [Fact]
        public void Ctor_NullFileSystem_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                this.sut = new JsonFileProductManifestRepository(null));
        }

        [Fact]
        public void GetManifest_NullProduct_ThrowsException()
        {
            IFileSystem fileSystem = Substitute.For<IFileSystem>();

            this.sut = new JsonFileProductManifestRepository(fileSystem);

            Assert.Throws<ArgumentException>(() => this.sut.GetManifest(null, "someVersion"));
        }

        [Fact]
        public void GetManifest_ValidJson_ReturnsManifest()
        {
            IFileSystem fileSystem = Substitute.For<IFileSystem>();

            fileSystem.OpenRead(Arg.Any<string>()).Returns(GetValidStream());

            this.sut = new JsonFileProductManifestRepository(fileSystem);
            ProductManifest result = this.sut.GetManifest("someProduct", "1.0.0");

            Assert.NotNull(result);
        }

        [Fact]
        public void GetManifest_InvalidJson_ThrowsException()
        {
            IFileSystem fileSystem = Substitute.For<IFileSystem>();

            fileSystem.OpenRead(Arg.Any<string>()).Returns(GetInvalidStream());

            this.sut = new JsonFileProductManifestRepository(fileSystem);

            Assert.Throws<JsonSerializationException>(() => this.sut.GetManifest("someProduct", "someVersion"));
        }

        [Fact]
        public void GetManifest_ProductNotFound_ReturnsNull()
        {
            IFileSystem fileSystem = Substitute.For<IFileSystem>();

            fileSystem.OpenRead(Arg.Any<string>()).Returns(GetValidStream());

            this.sut = new JsonFileProductManifestRepository(fileSystem);
            ProductManifest result = this.sut.GetManifest("foo", "1.0.0");

            Assert.Null(result);
        }

        [Fact]
        public void GetManifest_ManyProducts_GetsCorrectProduct()
        {
            IFileSystem fileSystem = Substitute.For<IFileSystem>();

            fileSystem.OpenRead(Arg.Any<string>()).Returns(GetValidStreamManyProducts());

            this.sut = new JsonFileProductManifestRepository(fileSystem);
            ProductManifest result = this.sut.GetManifest("otherProduct", "1.0.0");

            Assert.NotNull(result);
        }

        [Fact]
        public void GetManifest_VersionNotFound_ReturnsNull()
        {
            IFileSystem fileSystem = Substitute.For<IFileSystem>();

            fileSystem.OpenRead(Arg.Any<string>()).Returns(GetValidStream());

            this.sut = new JsonFileProductManifestRepository(fileSystem);
            ProductManifest result = this.sut.GetManifest("someProduct", "1.999.0");

            Assert.Null(result);
        }

        [Fact]
        public void GetManifest_ManyVersions_GetsCorrectVersion()
        {
             IFileSystem fileSystem = Substitute.For<IFileSystem>();

            fileSystem.OpenRead(Arg.Any<string>()).Returns(GetValidStreamManyVersions());

            this.sut = new JsonFileProductManifestRepository(fileSystem);
            ProductManifest result = this.sut.GetManifest("otherProduct", "1.1.0");

            Assert.NotNull(result);
        }

        private static Stream GetInvalidStream()
        {
            string badString = "{\"name\": \"someProduct\",\"services\": [{\"name\": \"someService1\",\"version\": \"1.0.1\"},";

            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(badString));
            stream.Position = 0;

            return stream;
        }

        private static Stream GetValidStream()
        {
            string goodString = "{\"productManifests\": [{\"name\": \"someProduct\",\"versions\": [{\"version\": \"1.0.0\",\"services\": [{\"name\": \"someService1\",\"version\": \"1.0.1\"},{\"name\": \"someService2\",\"version\": \"1.0.2\"}]}]}]}";

            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(goodString));
            stream.Position = 0;

            return stream;
        }

        private static Stream GetValidStreamManyProducts()
        {
            string goodString = "{\"productManifests\": [{\"name\": \"someProduct\",\"versions\": [{\"version\": \"1.0.0\",\"services\": [{\"name\": \"someService1\",\"version\": \"1.0.1\"},{\"name\": \"someService2\",\"version\": \"1.0.2\"}]}]},{\"name\": \"otherProduct\",\"versions\": [{\"version\": \"1.0.0\",\"services\": [{\"name\": \"someService1\",\"version\": \"1.0.1\"},{\"name\": \"someService2\",\"version\": \"1.0.2\"}]}]}]}";

            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(goodString));
            stream.Position = 0;

            return stream;
        }

        private static Stream GetValidStreamManyVersions()
        {
            string goodString = "{\"productManifests\": [{\"name\": \"someProduct\",\"versions\": [{\"version\": \"1.0.0\",\"services\": [{\"name\": \"someService1\",\"version\": \"1.0.1\"},{\"name\": \"someService2\",\"version\": \"1.0.2\"}]}]},{\"name\": \"otherProduct\",\"versions\": [{\"version\": \"1.1.0\",\"services\": [{\"name\": \"someService1\",\"version\": \"1.0.1\"},{\"name\": \"someService2\",\"version\": \"1.0.2\"}]},{\"version\": \"1.0.0\",\"services\": [{\"name\": \"someService1\",\"version\": \"1.0.1\"},{\"name\": \"someService2\",\"version\": \"1.0.2\"}]}]}]}";

            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(goodString));
            stream.Position = 0;

            return stream;
        }
    }
}
