namespace SolutionDeploy.Vsts
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Authentication;
    using System.Web;

    using Microsoft.Extensions.Logging;

    using SolutionDeploy.Core;

    public class VstsOAuthAuthenticator : IAuthenticator
    {
        private readonly IHttpClient httpClient;
        private readonly ITokenRepository tokenRepository;
        private readonly VstsConfig vstsConfig;
        private ILogger logger = Logging.GetLogger<VstsOAuthAuthenticator>();
        private OAuthAccessTokens tokens;

        public VstsOAuthAuthenticator(
            IHttpClient httpClient, ITokenRepository tokenRepository, VstsConfig vstsConfig)
        {
            if (httpClient == null) { throw new ArgumentNullException(nameof(httpClient)); }
            if (tokenRepository == null) { throw new ArgumentNullException(nameof(tokenRepository)); }
            if (vstsConfig == null) { throw new ArgumentNullException(nameof(vstsConfig)); }

            this.httpClient = httpClient;
            this.tokenRepository = tokenRepository;
            this.vstsConfig = vstsConfig;
        }

        public AuthenticationResult Authenticate()
        {
            this.tokens = this.tokenRepository.GetTokens();
            if (this.tokens == null)
            {
                throw new NoTokensException("Application not authorised. ", this.GetAuthorizationUrl());
            }

            if (this.IsExpired(this.tokens))
            {
                this.logger.LogDebug("oauth tokens expired, refreshing");
                try
                {
                    this.tokens = this.RefreshTokens();
                    this.tokenRepository.SaveTokens(this.tokens);
                }
                catch (Exception ex)
                {
                    return new AuthenticationResult { Exception = ex };
                }
            }

            return new AuthenticationResult
            {
                AccessToken = this.tokens.AccessToken,
                Success = true
            };
        }

        protected virtual OAuthAccessTokens GetTokensFromResponse(HttpWebResponse response)
        {
            return AuthorizationResponseReader.ReadTokenRefreshResponse(response);
        }

        protected virtual bool IsExpired(OAuthAccessTokens tokens)
        {
            return TokenValidityChecker.IsExpired(tokens, this.vstsConfig.TokenEarlyExpiry);
        }

        private static string GenerateTokenRefreshData(string refreshToken, string clientSecret, string callbackUrl)
        {
            return $"client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer" +
                $"&client_assertion={HttpUtility.UrlEncode(clientSecret)}" +
                $"&grant_type=refresh_token&assertion={HttpUtility.UrlEncode(refreshToken)}" +
                $"&redirect_uri={callbackUrl}";
        }

        private Uri GetAuthorizationUrl()
        {
            return new Uri("https://app.vssps.visualstudio.com/oauth2/authorize?client_id=" +
                $"{this.vstsConfig.AuthorizationClientId}" +
                $"&response_type=Assertion&state={this.vstsConfig.AuthorizationUserId}" +
                $"&scope={this.GetAuthorizedScopes()}&redirect_uri=" +
                $"{this.vstsConfig.AuthorizationCallbackUrl}");
        }

        private OAuthAccessTokens RefreshTokens()
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)this.httpClient
                    .Execute(this.GetRefreshRequest());

                return this.GetTokensFromResponse(response);
            }
            catch (Exception ex)
            {
                throw new AuthenticationException("could not refresh access token", ex);
            }
        }

        private HttpWebRequest GetRefreshRequest()
        {
            Uri refreshUri = new Uri(this.vstsConfig.TokenUrl);

            string refreshData = GenerateTokenRefreshData(
                this.tokens.RefreshToken,
                this.vstsConfig.AuthorizationClientSecret,
                this.vstsConfig.AuthorizationCallbackUrl);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(refreshUri);
            request.Method = "POST";
            request.ContentLength = refreshData.Length;
            request.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter streamtWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamtWriter.Write(refreshData);
            }

            return request;
        }

        private string GetAuthorizedScopes()
        {
            return HttpUtility.UrlPathEncode(string.Join(" ", this.vstsConfig.AuthorizedScopes));
        }
    }
}
