namespace PublicAPI.Samples
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;

    using Newtonsoft.Json;

    public class ApiV2Sample
    {
        private static string token;
        internal static void Run()
        {
            try
            {
                CollectCredentials();
                InitApiUrls();
                var contact = CreateNewContact();
                contact = UpdateContactFields(contact);
                var evnt = GetEvent();
                var registration = RegisterContactForEvent(evnt, contact);
                var payment = CreatePayment(registration.Invoice);
                DeletePayment(payment);
                DeleteRegistration(registration);
                ArchiveContact(contact);
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                var status = response.StatusCode;
                var reason = response.StatusDescription;
                Console.WriteLine("API returned error:{0} {1}", status, reason);

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var str = reader.ReadToEnd();
                    Console.WriteLine(str);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Application error:{0}", ex);
            }
        }

        private static void InitApiUrls()
        {
            string version2Url = GetApiDataList(ApiUrls.BaseApiUrl).First(v => v.Version == 2).Url;

            string accountsUrl = GetApiDataList(version2Url).First(r => r.Name == "Accounts").Url;

            var account = GetApiDataList(accountsUrl).First(); // we use only first account

            Console.WriteLine("You have access to account: {0}", account.PrimaryDomainName);
            var resources = account.Resources as IEnumerable<dynamic>;
            ApiUrls.ContactsUrl = resources.First(r => r.Name == "Contacts").Url;
            ApiUrls.EventsUrl = resources.First(r => r.Name == "Events").Url;
            ApiUrls.EventRegistrations = resources.First(r => r.Name == "Event registrations").Url;
            ApiUrls.Payments = resources.First(r => r.Name == "Payments").Url;
            ApiUrls.Tenders = resources.First(r => r.Name == "Tenders").Url;
        }

        private static IEnumerable<dynamic> GetApiDataList(object url, string method = "GET", object data = null)
        {
            return GetApiData(url, method, data) as IEnumerable<dynamic>;
        }
        private static dynamic GetApiData(object url, string method = "GET", object data = null)
        {
            if (method == "GET")
            {
                var result = System.Net.WebRequest.Create(url.ToString())
                    .AcceptJson()
                    .SetBearerAuth(token)
                    .GetResponse()
                    .DownloadJsonObject();
                return result;
            }
            else
            {
                var json = JsonConvert.SerializeObject(data);
                var result = System.Net.WebRequest.Create(url.ToString())
                    .AcceptJson()
                    .SetBearerAuth(token)
                    .SetJsonData(json, method)
                    .GetResponse()
                    .DownloadJsonObject();

                return result;
            }
        }

        private static void ArchiveContact(dynamic contact)
        {
            var instanceForArchive = new
            {
                Id = contact.Id, 
                FieldValues = new[]
                {
                    new
                    {
                        FieldName = "Archived", 
                        Value = true
                    }
                }
            };

            var archivedContact = GetApiData(contact.Url, "PUT", instanceForArchive);

            Console.WriteLine("Contact #{0} was archived.", archivedContact.Id);
        }

        private static void DeleteRegistration(dynamic registration)
        {
            GetApiData(registration.Url, "DELETE");

            Console.WriteLine("Registration of contact '{0}' for event '{1}' was deleted", registration.Contact.Name, registration.Event.Name);
        }

        private static void DeletePayment(dynamic payment)
        {
            GetApiData(payment.Url, "DELETE");

            Console.WriteLine("Payment #{0} was deleted", payment.Id);
        }

        private static dynamic CreatePayment(dynamic invoice)
        {
            var tender = GetApiDataList(ApiUrls.Tenders).First(t => t.Name == "PayPal Payments Standard");
            invoice = GetApiData(invoice.Url);

            var newPayment = new
            {
                Value = invoice.Value, 
                Invoices = new[]
                {
                    new
                    {
                        Id = invoice.Id
                    }
                }, 
                Tender = new
                {
                    Id = tender.Id
                }, 
                Comment = "Sample payment"
            };

            var createdPayment = GetApiData(ApiUrls.Payments, "POST", newPayment);

            Console.WriteLine("Payment for invoice #{0} (doc number {1}) was created", invoice.Id, invoice.DocumentNumber);

            return createdPayment;
        }

        private static dynamic RegisterContactForEvent(dynamic evnt, dynamic contact)
        {
            var regType = (evnt.Details.RegistrationTypes as IEnumerable<dynamic>)
                .OrderByDescending(r => r.BasePrice)
                .First(rt => rt.IsEnabled == true);

            var newRegistration = new
            {
                Event = new
                {
                    Id = evnt.Id
                }, 
                Contact = new
                {
                    Id = contact.Id
                }, 
                RegistrationTypeId = regType.Id, 
                RegistrationFields = new object[] { }, 
                RecreateInvoice = true
            };

            var createdRegistration = GetApiData(ApiUrls.EventRegistrations, "POST", newRegistration);
            Console.WriteLine("Registration of contact '{0}' for event '{1}' was created", contact.DisplayName, evnt.Name);
            return createdRegistration;
        }

        private static dynamic GetEvent()
        {
            var events = GetApiData(ApiUrls.EventsUrl).Events as IEnumerable<dynamic>;
            var evt = events.FirstOrDefault();

            if (evt == null)
            {
                throw new Exception("Can't find any event.");
            }

            // Request single event item to get event description with details section
            var result = GetApiData(evt.Url);

            Console.WriteLine("Event '{0}' found", result.Name);
            return result;
        }

        private static dynamic UpdateContactFields(dynamic contact)
        {
            var instanceForUpdate = new
            {
                Id = contact.Id, 
                FieldValues = new[]
                {
                    new
                    {
                        FieldName = "First name", 
                        Value = "Mike"
                    }
                }
            };

            var updatedContact = GetApiData(contact.Url, "PUT", instanceForUpdate);

            Console.WriteLine("Contact was updated: '{0}'.", updatedContact.DisplayName);
            return updatedContact;
        }

        private static dynamic CreateNewContact()
        {
            Console.WriteLine("Enter email for new contact:");
            var contact = new
            {
                Email = Console.ReadLine(), 
                FirstName = "John", 
                LastName = "Smith"
            };

            var createdContact = GetApiData(ApiUrls.ContactsUrl, "POST", contact);

            Console.WriteLine("New contact '{0}' was created.", createdContact.DisplayName);

            return createdContact;
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

            throw new NonImplementedException("Change clientId and clientSecret to values specific for your authirized application. For details see: http://help.wildapricot.com/display/DOC54/Authorizing+external+applications");
            
            var clientId = "MySampleApplication";
            var clientSecret = "open_wa_api_client";

            var response = System.Net.WebRequest.Create(ApiUrls.OAuthServiceUrl)
                .SetBasicAuth(clientId, clientSecret)
                .SetData(authData)
                .GetResponse();

            var oauthResponse = response.DownloadJsonObject();

            token = oauthResponse.access_token.ToString();

            Console.WriteLine("OAuth token obtained.");
        }
    }
}