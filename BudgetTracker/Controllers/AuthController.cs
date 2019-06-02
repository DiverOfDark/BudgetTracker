using System;
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
        public IActionResult Index(string returnUrl = null)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                returnUrl = ValidateReturnUrl(returnUrl);
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Widget");
            }
            
            ViewData["RenderBody"] = false;
            return View();
        }

        public async Task<IActionResult> Login(string password, string returnUrl)
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

                returnUrl = ValidateReturnUrl(returnUrl);
                
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Widget");
            }

            ViewData["RenderBody"] = false;
            ViewData["Error"] = "Неверный пароль!";
            return View(nameof(Index));
        }

        private string ValidateReturnUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;
            
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri) && uri.IsAbsoluteUri)
                return null;
            
            return url;
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Index));
        }
    }
}