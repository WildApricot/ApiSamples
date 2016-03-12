namespace OAuthClientTestApp.Model
{
    using System.Web.Mvc;

    public class TestEnvironmentModel
    {
        [AllowHtml]
        public int AccountId { get; set; }

        [AllowHtml]
        public string ClientId { get; set; }

        [AllowHtml]
        public string ClientSecret { get; set; }

        [AllowHtml]
        public string OAuthServiceUrl { get; set; }

        public string OAuthTokenEndpoint
        {
            get
            {
                return this.OAuthServiceUrl + "/auth/token";
            }
        }

        [AllowHtml]
        public string PublicApiUrl { get; set; }

        [AllowHtml]
        public string AssociationWebSiteUrl { get; set; }

        public string AuthFormUrl
        {
            get
            {
                return this.AssociationWebSiteUrl + "/sys/login/OAuthLogin";
            }
        }
        public string LogoutUrl
        {
            get
            {
                return this.AssociationWebSiteUrl + "/sys/login/logout";
            }
        }
        public string LogoutNonceUrl
        {
            get
            {
                return this.AssociationWebSiteUrl + "/sys/login/LogoutNonce";
            }
        }

        public string Scope { get; set; }
    }
}