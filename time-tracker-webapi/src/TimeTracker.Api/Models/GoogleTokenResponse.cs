using System;

namespace TimeTracker.Api.Models
{
    public class GoogleTokenResponse
    {
        public String access_token { get; set; }
        public String id_token { get; set; }
        public String refresh_token { get; set; }
        public int expires_in { get; set; }
        public String token_type { get; set; }
        
    }
}