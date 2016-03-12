namespace OAuthClientTestApp.Controllers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Security;

    using Newtonsoft.Json;

    using OAuthClientTestApp.Model;
    using OAuthClientTestApp.Utils;

    public class MainController : Controller
    {
        [System.Web.Mvc.HttpGet]
        public ActionResult Index()
        {
            

            var model = new TestEnvironmentModel()
            {
                AccountId = 1,
                ClientId = "set client id from settings / security / authorized applications",
                ClientSecret = "set client secret from settings / security / authorized applications",
                OAuthServiceUrl = "https://oauth.wildapricot.org",
                PublicApiUrl = "https://api.wildapricot.org",
                AssociationWebSiteUrl = "https://yourassociation.wildapricot.org",
                Scope = "auto"
            };

            var modelCookie = this.Request.Cookies["environment"];
            if (modelCookie != null)
            {
                model = JsonConvert.DeserializeObject<TestEnvironmentModel>(modelCookie.Value);
            }

            return this.View(model);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult StartOAuthLogin(TestEnvironmentModel model)
        {
            if (model != null)
            {
                this.Response.SetCookie(new HttpCookie("environment", JsonConvert.SerializeObject(model)));
            }

            var stateKey = Guid.NewGuid().ToString("N");
            this.Session.Add(stateKey, model);
            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters.Add("state", stateKey);

            var callBackUri = this.GetCallBackUrl();
            parameters.Add("redirect_uri", callBackUri);
            parameters.Add("scope", model.Scope);
            parameters.Add("client_id", model.ClientId);
            parameters.Add("response_type", "authorization_code");
            parameters.Add("claimed_account_id", model.AccountId.ToString());

            var uri = new UriBuilder(model.AuthFormUrl);
            uri.Query = parameters.ToString();
            return this.Redirect(uri.ToString());
        }
        private string GetCallBackUrl()
        {
            var callBackUri = new System.UriBuilder(this.Request.Url.AbsoluteUri)
            {
                Path = this.Url.Action("OAuthCallBack"),
                Query = null,
            };
            return callBackUri.ToString();
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult OAuthCallBack(string state, string code)
        {
            try
            {
                var model = this.Session[state] as TestEnvironmentModel;
                var token = this.GetToken(code, model);
                this.Session["token"] = token;

                var contactInfo = this.GetContactInfo(token, model);
                this.SetAuthCookie(contactInfo);
                return this.RedirectToAction("PrivateContent", new
                {
                    state
                });
            }
            catch (WebException ex)
            {
                var stream = ex.Response.GetResponseStream();
                using (var reader = new StreamReader(stream))
                {
                    var str = reader.ReadToEnd();
                    Trace.WriteLine(str);
                    throw;
                }
            }
        }

        private void SetAuthCookie(string email)
        {
            FormsAuthentication.SetAuthCookie(email, true);
        }

        private string GetContactInfo(string token, TestEnvironmentModel model)
        {
            var url = string.Format("{0}/v2/accounts/{1}/contacts/me", model.PublicApiUrl, model.AccountId);
            var contactInfo = System.Net.WebRequest.Create(url)
                .SetBearerAuth(token)
                .GetResponse()
                .DownloadJsonObject<dynamic>();

            return contactInfo.Email;
        }

        private string GetToken(string code, TestEnvironmentModel model)
        {
            var redirectUrl = this.GetCallBackUrl();
            // init
            var data = string.Format("grant_type={0}&code={1}&client_id={2}&redirect_uri={3}&scope={4}", "authorization_code", code, model.ClientId, redirectUrl, model.Scope);

            // execute
            var response = System.Net.WebRequest.Create(model.OAuthTokenEndpoint)
                .SetBasicAuth(model.ClientId, model.ClientSecret)
                .SetData(data)
                .GetResponse();

            var tokenData = response.DownloadJsonObject<dynamic>();
            var token = tokenData.access_token.ToString();
            return token as string;
        }

        [System.Web.Mvc.Authorize]
        public ActionResult PrivateContent(string state)
        {
            return this.View(new
            {
                UserName = this.User.Identity.Name,
                state
            }.ToExpando());
        }

        [HttpPost]
        public ActionResult LogOut(string state)
        {
            var redirectUrl = new System.UriBuilder(this.Request.Url.AbsoluteUri)
            {
                Path = this.Url.Action("Index"),
                Query = null,
            };

            var model = this.Session[state] as TestEnvironmentModel;
            var token = this.Session["token"] as string;
            var nonce = this.GetLogoutNonce(token, model, redirectUrl.ToString());

            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters.Add("nonce", nonce);
            var uri = new UriBuilder(model.LogoutUrl);
            uri.Query = parameters.ToString();

            FormsAuthentication.SignOut();

            return this.Redirect(uri.ToString());
        }

        private string GetLogoutNonce(string token, TestEnvironmentModel model, string redirectUrl)
        {
            var response = System.Net.WebRequest.Create(model.LogoutNonceUrl)
                .SetBasicAuth(model.ClientId, model.ClientSecret)
                .SetJsonData(JsonConvert.SerializeObject(
                    new
                    {
                        token,
                        email = this.User.Identity.Name,
                        redirectUrl
                    }
                    ))
                .GetResponse();

            var nonce = response.DownloadJsonObject<dynamic>().nonce.ToString();
            return nonce as string;
        }
    }
}