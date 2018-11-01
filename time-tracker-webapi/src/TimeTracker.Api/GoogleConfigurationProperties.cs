using System;

namespace TimeTracker.Api
{
    /// <summary>
    /// Config properties to tie up with your google organization. These all come from the google dev console
    /// </summary>
    public class GoogleConfigurationProperties
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