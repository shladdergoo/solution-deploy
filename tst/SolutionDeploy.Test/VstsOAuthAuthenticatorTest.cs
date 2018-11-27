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
                AuthorizationClientId = "07FB4ADB-216C-48D1-BD90-E218129CEB4D",
                AuthorizationClientSecret = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJjaWQiOiIwN2ZiNGFkYi0yMTZjLTQ4ZDEtYmQ5MC1lMjE4MTI5Y2ViNGQiLCJjc2kiOiI3NzRmNzAxOC03YWRkLTRjYzktOGJjMy03YTczNWU0NWVjNTciLCJuYW1laWQiOiIxZWFmZTE5ZS01ZGQ3LTQ4MmMtODJmYi03OGUyMzE0ZTYxNDciLCJpc3MiOiJhcHAudnNzcHMudmlzdWFsc3R1ZGlvLmNvbSIsImF1ZCI6ImFwcC52c3Nwcy52aXN1YWxzdHVkaW8uY29tIiwibmJmIjoxNTI4OTk2NzU1LCJleHAiOjE2ODY3NjMxNTV9.wvvoxDeBSKH4QzXUyWUAfmDmLwC3Si1GTeI_4XSd5tZhwEU3dYNiEFIvAfCAWrt81NPMAXGkpNrH7oogkyoW8NtnzhBiAo8z0PE2Aw8cVQh95JlNH1XNhd3G7yv0wpzETVP6xfHRpJo1bYPVJAHg2d9PSEty_jMN8CdWUxf3R6VUxw-CCQkiV8STCTc-GKtRFoESV_gwg2KryEExRr3BCvq8vAKV8ThD-aES250oqyJ8twzIzkatE7INvmuxJeIY81X3twV6q4EpfEaiaQ7KHF7gpO_5zf5coAXX0L829Hm5qlpnME8548Q5aWTtkoC6fGg2ldKxGhIlM3IPhYTNqA",
                AuthorizationCallbackUrl = "https://hcc-devops-authorization.azurewebsites.net/api/auth",
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
