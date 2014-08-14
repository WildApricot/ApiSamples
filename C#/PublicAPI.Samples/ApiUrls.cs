namespace PublicAPI.Samples
{
    internal static class ApiUrls
    {
        public const string BaseApiUrl = "https://api.wildapricot.org";
        public const string OAuthServiceUrl = "https://oauth.wildapricot.org/auth/token";

        public static string ContactsUrl { get; set; }

        public static string EventsUrl { get; set; }

        public static string EventRegistrations { get; set; }

        public static string Payments { get; set; }

        public static string Tenders { get; set; }
    }
}