using System;

namespace TimeTracker.Api
{
    public class ConfigurationProperties
    {
        public String GoogleOrgClientId { get; set; }
        public String GoogleOrgClientSecret { get; set; }
        
        /// <summary>
        /// One of the redirect URIs listed for your project in the Google API COnsole
        /// https://console.developers.google.com/
        /// </summary>
        public String GoogleOrgRedirectUri { get; set; }
    }
}