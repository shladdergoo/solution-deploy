namespace SolutionDeploy.Test
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    using NSubstitute;
    using Xunit;

    using SolutionDeploy.Core;
    using SolutionDeploy.Vsts;

    public class AuthorizationResponseReaderTest
    {
        [Fact]
        public void GetTokenRefreshResponse_GetsTokens()
        {
            HttpWebResponse response = Substitute.For<HttpWebResponse>();
            response.GetResponseStream().Returns(new MemoryStream(GetTestRefreshBody()));

            OAuthAccessTokens result = AuthorizationResponseReader.ReadTokenRefreshResponse(response);

            Assert.NotNull(result);
        }

        private static byte[] GetTestRefreshBody()
        {
            return Encoding.UTF8.GetBytes("{\"access_token\":\"eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJuYW1laWQiOiIxZWFmZTE5ZS01ZGQ3LTQ4MmMtODJmYi03OGUyMzE0ZTYxNDciLCJzY3AiOiJ2c28uYnVpbGRfZXhlY3V0ZSB2c28ucmVsZWFzZV9tYW5hZ2UiLCJpc3MiOiJhcHAudnNzcHMudmlzdWFsc3R1ZGlvLmNvbSIsImF1ZCI6ImFwcC52c3Nwcy52aXN1YWxzdHVkaW8uY29tIiwibmJmIjoxNTI3MTcyNjY2LCJleHAiOjE1MjcxNzYyNjZ9.KHLz_z5P4_wEiy2xV3EJ1KDS0GsYUgbM9MvvNyJXpMym5udalnOhAfb7U1BAq_qs84koClCwXO3YQ2369wsOrozHH9htBnP5Hnteb05MW3ALnf8GdxXrS68o4jmEhl75TIlUNq7nZ7mVCwKnNlmLGx7bJxh5_bZS49EpAXSue4jzcoa6y3_KAdxIfYESZqB-zIp7BLERwXZEQSAXqTjYUTdQCzKdMQM8rz2oAEam-8t2Ix_1riTEshvJreLYJKAAxmid3Ad-chCMDLzVO3dpnHkEleUPpP0dKe5IcD15cBLG9dBudJHg5rGn6kaUMDOTh37k7259Go6f6KlEjzaKkg\",\"token_type\":\"jwt-bearer\",\"expires_in\":\"3600\",\"refresh_token\":\"eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJuYW1laWQiOiIxZWFmZTE5ZS01ZGQ3LTQ4MmMtODJmYi03OGUyMzE0ZTYxNDciLCJhY2kiOiI0ZWM0Y2JmMi1hNjNiLTRhNGQtYmZiYS01MmNjZWRjODc5M2EiLCJzY3AiOiJ2c28uYnVpbGRfZXhlY3V0ZSB2c28ucmVsZWFzZV9tYW5hZ2UiLCJpc3MiOiJhcHAudnNzcHMudmlzdWFsc3R1ZGlvLmNvbSIsImF1ZCI6ImFwcC52c3Nwcy52aXN1YWxzdHVkaW8uY29tIiwibmJmIjoxNTI3MTcyNjY2LCJleHAiOjE1NTg3MDg2NjZ9.x_dZQiEQXX9M67FsaBnk3jWQvtNelc0D67-rZynDcqGmE7Dq_ZodbIMKhDwh7ikqa-h8E5cRtSUISh3w7tCxmUSht1yjEUbX1jOmK6Q3gF-Zk4O0IsUzgjmOnyUoqf_ZdW5FONngddCA42gj-s-6m1cG0oll1ExOZ9sgigS7elEmZVRG0jbtW_aTGXVOpMvNMkaARsgoyJGZaO2YAyMuBeTSOO9v9Pe2mQ1YVs0MeWQjQpgt_CNuzxM_sCnoONz_f7fDOqlNJ6FOAcakiB8VoQO5e5xklllK1_ytO225GK6nCvGv6Y3Hfvj9RXfmjAjepAt7faTge-QHx7WTLTJU3A\",\"scope\":\"vso.build_execute vso.release_manage\"}");
        }
    }
}
