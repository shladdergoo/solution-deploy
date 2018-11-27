namespace SolutionDeploy.Test
{
    using System;

    using Xunit;

    using SolutionDeploy.Core;

    public class ProductVersionTest
    {
        ProductVersion sut;

        [Fact]
        public void Ctor_NullServices_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                this.sut = new ProductVersion("someVersion", null));
        }
    }
}