namespace SolutionDeploy.Test
{
    using System;
    using System.Net;
    using System.Security.Authentication;

    using NSubstitute;
    using Xunit;

    using SolutionDeploy.Core;
    using SolutionDeploy.Vsts;

    public class VstsOAuthAuthenticatorTest
    {
        IAuthenticator sut;

        [Fact]
        public void Ctor_NullHttpClient_ThrowsException()
        {
            ITokenRepository tokenRepository = Substitute.For<ITokenRepository>();
            VstsConfig vstsConfig = GetVstsConfig();

            Assert.Throws<ArgumentNullException>(() =>
                this.sut = new VstsOAuthAuthenticator(null, tokenRepository, vstsConfig));
        }

        [Fact]
        public void Ctor_NullTokenRepository_ThrowsException()
        {
            IHttpClient httpClient = Substitute.For<IHttpClient>();
            VstsConfig vstsConfig = GetVstsConfig();

            Assert.Throws<ArgumentNullException>(() =>
                this.sut = new VstsOAuthAuthenticator(httpClient, null, vstsConfig));
        }

        [Fact]
        public void Ctor_NullVstsConfig_ThrowsException()
        {
            ITokenRepository tokenRepository = Substitute.For<ITokenRepository>();
            IHttpClient httpClient = Substitute.For<IHttpClient>();

            Assert.Throws<ArgumentNullException>(() =>
                this.sut = new VstsOAuthAuthenticator(httpClient, tokenRepository, null));
        }

        [Fact]
        public void Authenticate_NoTokens_ThrowsException()
        {
            ITokenRepository tokenRepository = Substitute.For<ITokenRepository>();
            IHttpClient httpClient = Substitute.For<IHttpClient>();
            VstsConfig vstsConfig = GetVstsConfig();

            tokenRepository.GetTokens().Returns((OAuthAccessTokens)null);

            this.sut = new VstsOAuthAuthenticator(httpClient, tokenRepository, vstsConfig);

            Assert.Throws<NoTokensException>(() => this.sut.Authenticate());
        }

        [Fact]
        public void Authenticate_AccessTokenValid_ReturnsToken()
        {
            ITokenRepository tokenRepository = Substitute.For<ITokenRepository>();
            IHttpClient httpClient = Substitute.For<IHttpClient>();
            VstsConfig vstsConfig = GetVstsConfig();

            tokenRepository.GetTokens().Returns(new OAuthAccessTokens()
            {
                AccessToken = "foo",
                RefreshToken = "bar"
            });

            this.sut = new TestableVstsOAuthAuthenticator_NotExpired(
                httpClient, tokenRepository, vstsConfig);

            AuthenticationResult result = this.sut.Authenticate();

            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);
            Assert.True(result.Success);
        }

        [Fact]
        public void Authenticate_AccessTokenExpired_RefreshesToken()
        {
            ITokenRepository tokenRepository = Substitute.For<ITokenRepository>();
            IHttpClient httpClient = Substitute.For<IHttpClient>();
            VstsConfig vstsConfig = GetVstsConfig();
            HttpWebResponse response = Substitute.For<HttpWebResponse>();
            response.StatusCode.Returns(HttpStatusCode.OK);

            tokenRepository.GetTokens().Returns(new OAuthAccessTokens()
            {
                AccessToken = "foo",
                RefreshToken = "bar"
            });

            httpClient.Execute(Arg.Any<HttpWebRequest>()).Returns(response);

            this.sut = new TestableVstsOAuthAuthenticator_Expired(
                httpClient, tokenRepository, vstsConfig);

            AuthenticationResult result = this.sut.Authenticate();

            httpClient.Received().Execute(
                Arg.Is<HttpWebRequest>(
                    r => r.Method == "POST" && r.RequestUri.GetLeftPart(UriPartial.Path) ==
                        vstsConfig.TokenUrl));
        }

        [Fact]
        public void Authenticate_RefreshesTokens_ReturnsNewTokens()
        {
            ITokenRepository tokenRepository = Substitute.For<ITokenRepository>();
            IHttpClient httpClient = Substitute.For<IHttpClient>();
            VstsConfig vstsConfig = GetVstsConfig();
            HttpWebResponse response = Substitute.For<HttpWebResponse>();
            response.StatusCode.Returns(HttpStatusCode.OK);

            tokenRepository.GetTokens().Returns(new OAuthAccessTokens()
            {
                AccessToken = "foo",
                RefreshToken = "bar"
            });

            httpClient.Execute(Arg.Any<HttpWebRequest>()).Returns(response);

            this.sut = new TestableVstsOAuthAuthenticator_Expired(
                httpClient, tokenRepository, vstsConfig);

            AuthenticationResult result = this.sut.Authenticate();

            Assert.True(result.Success);
            Assert.NotNull(result.AccessToken);
        }

        [Fact]
        public void Authenticate_RefreshesTokens_SavesNewTokens()
        {
            ITokenRepository tokenRepository = Substitute.For<ITokenRepository>();
            IHttpClient httpClient = Substitute.For<IHttpClient>();
            VstsConfig vstsConfig = GetVstsConfig();
            HttpWebResponse response = Substitute.For<HttpWebResponse>();
            response.StatusCode.Returns(HttpStatusCode.OK);

            tokenRepository.GetTokens().Returns(new OAuthAccessTokens()
            {
                AccessToken = "foo",
                RefreshToken = "bar"
            });

            httpClient.Execute(Arg.Any<HttpWebRequest>()).Returns(response);

            this.sut = new TestableVstsOAuthAuthenticator_Expired(
                httpClient, tokenRepository, vstsConfig);

            AuthenticationResult result = this.sut.Authenticate();

            tokenRepository.ReceivedWithAnyArgs().SaveTokens(default(OAuthAccessTokens));
        }

        [Fact]
        public void Authenticate_CantRefreshToken_ReturnsBadResult()
        {
            ITokenRepository tokenRepository = Substitute.For<ITokenRepository>();
            IHttpClient httpClient = Substitute.For<IHttpClient>();
            VstsConfig vstsConfig = GetVstsConfig();
            HttpWebResponse response = Substitute.For<HttpWebResponse>();
            response.StatusCode.Returns(HttpStatusCode.BadRequest);

            tokenRepository.GetTokens().Returns(new OAuthAccessTokens()
            {
                AccessToken = "foo",
                RefreshToken = "bar"
            });

            httpClient.Execute(Arg.Any<HttpWebRequest>()).Returns(x => throw new WebException());

            this.sut = new TestableVstsOAuthAuthenticator_Expired(
                httpClient, tokenRepository, vstsConfig);
            AuthenticationResult result = this.sut.Authenticate();

            Assert.False(result.Success);
            Assert.IsType<AuthenticationException>(result.Exception);
        }

        private static VstsConfig GetVstsConfig()
        {
            return new VstsConfig
            {
                TokenUrl = @"https://app.vssps.visualstudio.com/oauth2/token",
                AuthorizationClientId = "",
                AuthorizationClientSecret = "",
                AuthorizationCallbackUrl = "",
                AuthorizationUserId = "SolutionDeployConsole",
                AuthorizedScopes = new string[]{"vso.build_execute", "vso.release_manage"}
            };
        }

        private static HttpWebRequest GetRequest()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("foo");

            return request;
        }
    }
}
