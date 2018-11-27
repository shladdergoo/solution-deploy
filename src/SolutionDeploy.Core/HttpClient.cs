namespace SolutionDeploy.Core
{
    using System.Net;

    public class HttpClient : IHttpClient
    {
        public HttpWebResponse Execute(HttpWebRequest webRequest)
        {
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

            return response;
        }
    }
}
