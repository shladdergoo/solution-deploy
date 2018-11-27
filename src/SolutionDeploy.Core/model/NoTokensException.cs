namespace SolutionDeploy.Core
{
    using System;

    public class NoTokensException : System.Exception
    {
        public NoTokensException()
        {
        }

        public NoTokensException(string message)
            : base(message)
        {
        }

        public NoTokensException(Uri authorizationUrl)
        {
            this.AuthorizationUrl = authorizationUrl;
        }

        public NoTokensException(string message, Uri authorizationUrl)
            : base(message)
        {
            this.AuthorizationUrl = authorizationUrl;
        }

        public NoTokensException(string message, System.Exception inner)
            : base(message, inner)
        {
        }

        public NoTokensException(string message, Uri authorizationUrl, System.Exception inner)
            : base(message, inner)
        {
            this.AuthorizationUrl = authorizationUrl;
        }

        protected NoTokensException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
                : base(info, context)
        {
        }

        public Uri AuthorizationUrl { get; }
    }
}
