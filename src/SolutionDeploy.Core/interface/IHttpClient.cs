namespace SolutionDeploy.Core
{
    using System.Net;

    public interface IHttpClient
    {
        HttpWebResponse Execute(HttpWebRequest webRequest);
    }
}
