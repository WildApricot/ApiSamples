namespace OAuthClientTestApp.Utils
{
    using System.IO;
    using System.Net;

    using Newtonsoft.Json;

    public static class WebResponseExtensions
    {
        public static T DownloadJsonObject<T>(this WebResponse response)
        {
            var stream = response.GetResponseStream();
            using (var reader = new StreamReader(stream))
            {
                var str = reader.ReadToEnd();
                var oauthObject = JsonConvert.DeserializeObject<T>(str);
                response.Dispose();
                return oauthObject;
            }
        }

        ////public static dynamic DownloadJsonObject(this WebResponse response)
        ////{
        ////    var stream = response.GetResponseStream();
        ////    using (var reader = new StreamReader(stream))
        ////    {
        ////        var str = reader.ReadToEnd();
        ////        var oauthObject = JsonConvert.DeserializeObject(str) as dynamic;
        ////        response.Dispose();
        ////        return oauthObject;
        ////    }
        ////}

        public static string DownloadString(this WebResponse response)
        {
            var stream = response.GetResponseStream();
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}