using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Data;
using TimeTracker.Library.Services;

namespace TimeTracker.Api.Controllers
{
    [Route("account"), Authorize]
    public class AccountController : Controller
    {
        private readonly TimeTrackerDbContext dbContext;
        private readonly UserService userService;
        private const string url = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/";

        public AccountController(TimeTrackerDbContext dbContext)
        {
            this.dbContext = dbContext;
            userService = new UserService(dbContext);
        }


        [HttpGet("linkslack")]
        public async Task<IActionResult> LinkSlack(string slackUser)
        {
            var ident = User.Identity as ClaimsIdentity;
            var user = ident.Claims.First(x => x.Type == url + "nameidentifier");
            string googleId = user.Value;

            string first = ident.Claims.First(x => x.Type == url + "givenname").Value;
            string last = ident.Claims.First(x => x.Type == url + "surname").Value;    
            string email = ident.Claims.First(x => x.Type == url + "emailaddress").Value;

            await userService.SaveGoogleInfo(slackUser, googleId, first, last, email);

            return RedirectToAction("Index", "Home");
        }
    }
}