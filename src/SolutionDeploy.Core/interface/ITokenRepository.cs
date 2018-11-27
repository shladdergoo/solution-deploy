namespace SolutionDeploy.Core
{
    public interface ITokenRepository
    {
        OAuthAccessTokens GetTokens();

        void SaveTokens(OAuthAccessTokens oAuthAccessTokens);
    }
}
