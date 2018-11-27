namespace SolutionDeploy.Test
{
    using System;

    using Xunit;

    using SolutionDeploy.Core;

    public class ManifestTest
    {
        Manifest sut;

        [Fact]
        public void Ctor_NullProducts_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => this.sut = new Manifest(null));
        }
    }
}
