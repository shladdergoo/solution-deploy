namespace SolutionDeploy.Core
{
    using System;

    public class VstsConfig
    {
        public string BaseUrl { get; set; }

        public string ProjectName { get; set; }

        public string TokenUrl { get; set; }

        public string AuthorizationClientId { get; set; }

        public string AuthorizationClientSecret { get; set; }

        public string AuthorizationCallbackUrl { get; set; }

        public int TokenEarlyExpiry { get; set; }

        public string AuthorizationUserId { get; set; }

        public string[] AuthorizedScopes { get; set; }
    }
}
