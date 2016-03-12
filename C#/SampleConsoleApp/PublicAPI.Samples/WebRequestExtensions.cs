namespace PublicAPI.Samples
{
    using System.IO;
    using System.Net;
    using System.Text;

    public static class WebRequestExtensions
    {
        public const string MimeJson = "application/json";
        public const string MimeFormUrlEncoded = "application/x-www-form-urlencoded";
        public static WebRequest SetJsonData(this WebRequest request, string data, string method = "POST")
        {
            return SetData(request, data, MimeJson, method);
        }

        public static WebRequest SetData(this WebRequest request, string data, string contentType = MimeFormUrlEncoded, string method = "POST")
        {
            request.Method = method;
            request.ContentType = contentType;
            byte[] sentData = Encoding.UTF8.GetBytes(data);

            request.ContentLength = sentData.Length;
            Stream sendStream = request.GetRequestStream();
            sendStream.Write(sentData, 0, sentData.Length);
            sendStream.Close();

            return request;
        }

        public static WebRequest SetBasicAuth(this WebRequest request, string userName, string password)
        {
            var credintials = string.Format("{0}:{1}", userName, password);
            var encoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(credintials));
            request.Headers.Add("Authorization", "Basic " + encoded);
            return request;
        }

        public static WebRequest SetBearerAuth(this WebRequest request, string token)
        {
            request.Headers.Add("Authorization", "Bearer " + token);
            return request;
        }

        public static WebRequest AcceptJson(this WebRequest request)
        {
            request.ContentType = MimeJson;
            return request;
        }
    }
}