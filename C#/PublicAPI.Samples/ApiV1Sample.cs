namespace PublicAPI.Samples
{
    using System;
    using System.IO;
    using System.Net;

    using Newtonsoft.Json;

    internal class ApiV1Sample
    {
        private const string CONST_BaseUrl = "https://api.wildapricot.org/";

        private static string accountId;
        private static string apiKey;

        internal static void Run()
        {
            ReadApiKey();
            ConnectToEntryPoint();
            LoadVersionResources();
            LoadAccountsList();
            LoadContactFields();
            LoadMembershipLevels();
            LoadContacts();
        }

        private static void ReadApiKey()
        {
            Console.Write("Enter api key:");
            apiKey = Console.ReadLine();
        }

        /// <summary>
        /// This helper method retrieves data from API and parse it to dynamic object.
        /// </summary>
        private static dynamic LoadObject(string url)
        {
            object result;
            var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.Accept, "application/json");
            client.Headers.Add("APIKey", apiKey);

            var stream = client.OpenRead(url);
            using (var reader = new StreamReader(stream))
            {
                var str = reader.ReadToEnd();
                result = JsonConvert.DeserializeObject(str);
            }
            return result as dynamic;
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

            var versionResources = LoadObject(CONST_BaseUrl + "/v1");
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

            var accounts = LoadObject(CONST_BaseUrl + "/v1/accounts");
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

            var url = string.Format("{0}/v1/accounts/{1}/ContactFields/", CONST_BaseUrl, accountId);
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

            var url = string.Format("{0}/v1/accounts/{1}/MembershipLevels/", CONST_BaseUrl, accountId);
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

            // filter all members, who has mail at gmail.com
            // 'email' are system fields, but it's name can be modified by account admin. 
            // If you have problem running program with this field name, check actual name in admin view or just set filterExpression = "$filter=Member eq true"
            var filterExpression = "$filter=Member eq true and substringof('e-Mail','@gmail.com')";

            // retrieve only some intresting data. Leave it empty to get all information about your contacts.
            // 'First name',Phone,'email' are system fields, but their names can be modified by account admin. 
            // If you have problem running program with these fields names, check actual names in admin view or just set selectExpression = String.Empty
            var selectExpression = "$select='First name',Phone,'email','Member since','Member Id'";

            // build url
            var url = string.Format("{0}/v1/accounts/{1}/Contacts/?{2}&{3}", CONST_BaseUrl, accountId, filterExpression, selectExpression);
            var request = LoadObject(url);

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
    }
}