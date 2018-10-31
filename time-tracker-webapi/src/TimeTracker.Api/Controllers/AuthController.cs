using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TimeTracker.Api.Models;

namespace TimeTracker.Api.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ConfigurationProperties _config;
        private const string GoogleTokenExchangeUrl = "https://www.googleapis.com/oauth2/v4/token";

        public AuthController(IConfiguration configuration)
        {
            _config = new ConfigurationProperties()
            {
                GoogleOrgClientId = configuration["GoogleSection:ClientId"],
                GoogleOrgClientSecret = configuration["GoogleSection:ClientSecret"],
                GoogleOrgRedirectUri = configuration["GoogleSection:RedirectUri"]
            };
        }
        
        [HttpGet("google/exchange")]
        public async Task<IActionResult> ExchangeGoogleAuthCode(string authorizationCode, string codeVerifier)
        {
            CancellationToken cancellationToken = new CancellationToken();
            using (var client = new HttpClient())
            using (var response = await client.PostAsJsonAsync(GoogleTokenExchangeUrl, new
            {
                code = authorizationCode,
                client_id = _config.GoogleOrgClientId,
                client_secret = _config.GoogleOrgClientSecret,
                redirect_uri = _config.GoogleOrgRedirectUri,
                grant_type = "authorization_code",
                code_verifier = codeVerifier
            }, cancellationToken))
            {
                // higher perf when use Streams
                var stream = await response.Content.ReadAsStreamAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var token = Utilities.DeserializeJsonFromStream<GoogleTokenResponse>(stream);
                    return Ok(token);
                }

                String errorContent = await Utilities.StreamToStringAsync(stream);
                throw new ExternalApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = errorContent,
                    EntityDescription = "Get Google auth token from authorization code"
                };
            }
        }
        
        
    }
}