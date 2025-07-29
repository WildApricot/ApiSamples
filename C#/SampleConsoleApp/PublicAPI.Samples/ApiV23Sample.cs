namespace PublicAPI.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;


    internal class ApiV23Sample
    {
        private const string CONST_BaseUrl = "https://api.wildapricot.org/";
        private static readonly HttpClient httpClient = new HttpClient();
        private static string accountId;
        private static string token;
        private const int PageSize = 100;


        internal static void Run()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            CollectCredentials();
            ConnectToEntryPoint();
            LoadVersionResources();
            LoadAccountsList();
            LoadContactFields();
            LoadMembershipLevels();
            LoadContacts();
        }
        private static void CollectCredentials()
        {
            Console.Write("Login:");
            var login = Console.ReadLine().Trim();
            Console.Write("Password:");
            var password = Console.ReadLine().Trim();

            var authData = string.Format("grant_type={0}&username={1}&password={2}&scope={3}",
                "password",
                login,
                password,
                "auto");


            throw new NotImplementedException("Change clientId and clientSecret to values specific for your authirized application. For details see: https://help.wildapricot.com/display/DOC/Authorizing+external+applications");

            var clientId = "<clientId>";
            var clientSecret = "<clientSecret>";

            var response = System.Net.WebRequest.Create(ApiUrls.OAuthServiceUrl)
                .SetBasicAuth(clientId, clientSecret)
                .SetData(authData)
                .GetResponse();

            var oauthResponse = response.DownloadJsonObject();

            token = oauthResponse.access_token.ToString();

            Console.WriteLine("OAuth token obtained.");
        }

        /// <summary>
        /// This helper method retrieves data from API and parse it to dynamic object.
        /// </summary>
        public static dynamic LoadObject(string baseUrl)
        {
            int offset = 0;
            bool hasMore = true;
            var allResults = new List<JToken>();
            const int PageSize = 100;

            while (hasMore)
            {
                string pagingParams = $"top={PageSize}&skip={offset}";
                string url = baseUrl.Contains("?") ? $"{baseUrl}&{pagingParams}" : $"{baseUrl}?{pagingParams}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using (var response = httpClient.SendAsync(request).Result)
                {
                    response.EnsureSuccessStatusCode();
                    string content = response.Content.ReadAsStringAsync().Result;

                    var json = JsonConvert.DeserializeObject(content);

                    // CASE A: Paged Array
                    if (json is JArray array)
                    {
                        allResults.AddRange(array);
                        hasMore = array.Count == PageSize;
                    }
                    // CASE B: Result Object
                    else if (json is JObject obj)
                    {
                        // Case: It contains a ResultUrl and a State
                        if (obj.ContainsKey("ResultUrl") && obj.ContainsKey("State"))
                        {
                            return obj; // Return the object as-is (not a list)
                        }

                        // Case: It contains a 'data' array inside
                        if (obj.TryGetValue("data", out var dataToken) && dataToken is JArray dataArray)
                        {
                            allResults.AddRange(dataArray);
                            hasMore = dataArray.Count == PageSize;
                        }
                        else
                        {
                            hasMore = false;
                        }
                    }
                    else
                    {
                        hasMore = false;
                    }
                }

                offset += PageSize;
            }

            return allResults;
        }

        public static void ConnectToEntryPoint()
        {
            Console.WriteLine();
            Console.WriteLine("Connecting to API entry point");
            var entryPointResources = LoadObject(CONST_BaseUrl);
            foreach (var resource in entryPointResources)
            {
                Console.WriteLine("Version:" + resource.Version);
                Console.WriteLine("Name:" + resource.Name);
                Console.WriteLine("Url:" + resource.Url);
            }
        }

        public static void LoadVersionResources()
        {
            Console.WriteLine();
            Console.WriteLine("Loading version resources");

            var versionResources = LoadObject(CONST_BaseUrl + "v2.3");
            foreach (var resource in versionResources)
            {
                Console.WriteLine("Name:" + resource.Name);
                Console.WriteLine("Url:" + resource.Url);
            }
        }

        public static void LoadAccountsList()
        {
            Console.WriteLine();
            Console.WriteLine("Loading account info");

            var accounts = LoadObject(CONST_BaseUrl + "/v2.3/accounts");
            foreach (var account in accounts)
            {
                Console.WriteLine("Id:" + account.Id);
                Console.WriteLine("Url:" + account.Url);
                Console.WriteLine("Name:" + account.Name);
                Console.WriteLine("PrimaryDomainName:" + account.PrimaryDomainName);

                accountId = account.Id.ToString();

                Console.WriteLine("Resources");
                foreach (var resource in account.Resources)
                {
                    Console.WriteLine("  Name:" + resource.Name);
                    Console.WriteLine("  Url:" + resource.Url);
                    Console.WriteLine("  ------");
                }
            }
        }

        public static void LoadContactFields()
        {
            Console.WriteLine();
            Console.WriteLine("Loading contact fields description");

            var url = string.Format("{0}/v2.3/accounts/{1}/ContactFields/", CONST_BaseUrl, accountId);
            var contactFields = LoadObject(url);

            foreach (var contactField in contactFields)
            {
                Console.WriteLine("FieldName:" + contactField.FieldName);
                Console.WriteLine("Type:" + contactField.Type);
                Console.WriteLine("Description:" + contactField.Description);
                Console.WriteLine("FieldInstructions:" + contactField.FieldInstructions);

                if (contactField.AllowedValues != null)
                {
                    Console.WriteLine("Allowed values");
                    foreach (var allowedValue in contactField.AllowedValues)
                    {
                        Console.WriteLine("  Id: {0}, Label: {1}", allowedValue.Id, allowedValue.Label);
                    }
                }
                Console.WriteLine("------");
            }
        }

        public static void LoadMembershipLevels()
        {
            Console.WriteLine();
            Console.WriteLine("Loading membership levels");

            var url = string.Format("{0}/v2.3/accounts/{1}/MembershipLevels/", CONST_BaseUrl, accountId);
            var levels = LoadObject(url);

            foreach (var level in levels)
            {
                Console.WriteLine("Id:" + level.Id);
                Console.WriteLine("Name:" + level.Name);
                Console.WriteLine("Type:" + level.Type);
                Console.WriteLine("MembershipFee:" + level.MembershipFee);
                Console.WriteLine("------");
            }
        }

        public static void LoadContacts()
        {
            Console.WriteLine();
            Console.WriteLine("Loading contacts");
            bool asyncRequest = true;
            // filter all members, who has mail at gmail.com
            // 'email' are system fields, but it's name can be modified by account admin. 
            // If you have problem running program with this field name, check actual name in admin view or just set filterExpression = "$filter=Member eq true"
            //var filterExpression = "$filter=Member eq true and substringof('Email','@gmail.com')";
            var filterExpression = "$filter=Member eq true";

            // retrieve only some intresting data. Leave it empty to get all information about your contacts.
            // 'First name',Phone,'email' are system fields, but their names can be modified by account admin. 
            // If you have problem running program with these fields names, check actual names in admin view or just set selectExpression = String.Empty
            var selectExpression = "$select='First name',Phone,'email','Member since','Member Id'";

            // build url
            var url = string.Format("{0}/v2.3/accounts/{1}/Contacts/?$async={4}&{2}&{3}", CONST_BaseUrl, accountId, filterExpression, selectExpression, asyncRequest);
            var request = LoadObject(url);

            if (asyncRequest)
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(3000);

                    request = LoadObject(request.ResultUrl.ToString());
                    string state = request.State.ToString();
                    Console.WriteLine("Request state is '{0}' at {1}", state, DateTime.Now);

                    switch (state)
                    {
                        case "Complete":
                            {
                                foreach (var contact in request.Contacts)
                                {
                                    Console.WriteLine("Contact #{0}:", contact.Id);
                                    foreach (var field in contact.FieldValues)
                                    {
                                        Console.WriteLine("  {0}: {1}", field.FieldName, field.Value);
                                    }
                                    Console.WriteLine("------");
                                }
                                return;
                            }
                        case "Failed":
                            {
                                Console.WriteLine("Error:'{0}'", request.ErrorDetails);
                                return;
                            }
                    }
                }
            }
            else {
                foreach (var contact in request)
                {
                    Console.WriteLine("Contact #{0}:", contact.Id);
                    foreach (var field in contact.FieldValues)
                    {
                        Console.WriteLine("  {0}: {1}", field.FieldName, field.Value);
                    }
                    Console.WriteLine("------");
                }
            }
        }
    }
}