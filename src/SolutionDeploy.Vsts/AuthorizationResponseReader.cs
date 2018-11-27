namespace SolutionDeploy.Vsts
{
    using System;
    using System.IO;
    using System.Net;

    using Newtonsoft.Json;

    using SolutionDeploy.Core;

    public static class AuthorizationResponseReader
    {
        public static OAuthAccessTokens ReadTokenRefreshResponse(HttpWebResponse response)
        {
            StreamReader streamReader = new StreamReader(response.GetResponseStream());

            OAuthAccessTokens tokens = null;
            using (JsonTextReader textReader = new JsonTextReader(streamReader))
            {
                JsonSerializer serializer = new JsonSerializer();
                tokens = serializer.Deserialize<OAuthAccessTokens>(textReader);
                tokens.Acquired = DateTime.UtcNow;
            }

            return tokens;
        }
    }
}
