namespace SolutionDeploy.Test
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    using SolutionDeploy.Core;

    public class ProductManifestTest
    {
        ProductManifest sut;

        [Fact]
        public void Ctor_NullName_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                this.sut = new ProductManifest(null, null, GetTestVerion(1)));
        }

        [Fact]
        public void Ctor_NullServices_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => this.sut = new ProductManifest("someProduct", null, null));
        }

        private static IEnumerable<ProductVersion> GetTestVerion(int count)
        {
            IList<Service> services = new List<Service>();
            for (int i = 0; i < count; i++)
            {
                services.Add(new Service($"service{i}", "someVersion"));
            }

            return new ProductVersion[] { new ProductVersion("someVersion", services) };
        }
    }
}
