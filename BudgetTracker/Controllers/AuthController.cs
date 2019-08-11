using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutCode.EscapeTeams.ObjectRepository;

namespace BudgetTracker.Controllers
{
    public class AuthController : Controller
    {
        [HttpPost]
        public async Task<bool> Login(string password)
        {
            if (string.Equals(Startup.GlobalSettings.Password ?? string.Empty, password ?? string.Empty))
            {
                var claims = new List<Claim>
                {
                    new Claim("PasswordHash", password.ToMD5())
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return true;
            }

            return false;
        }

        [Authorize, HttpPost]
        public async Task<OkResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

    }
}