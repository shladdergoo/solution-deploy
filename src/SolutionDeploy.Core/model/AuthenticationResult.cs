namespace SolutionDeploy.Core
{
    using System;

    public class AuthenticationResult
    {
        public bool Success { get; set; }

        public string AccessToken { get; set; }

        public Exception Exception { get; set; }
    }
}
