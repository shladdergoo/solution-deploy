namespace SolutionDeploy.Test
{
    using System;
    using Xunit;

    using SolutionDeploy.Core;

    public class ServiceTest
    {
        Service sut;

        [Fact]
        public void Ctor_NullName_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => this.sut = new Service(null, "someVersion"));
        }
    }
}
