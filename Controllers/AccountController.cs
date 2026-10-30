
using aznews.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace aznews.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataContext _context;
        private readonly PasswordHasher<User> _hasher;

        public AccountController(DataContext context)
        {
            _context = context;
            _hasher = new PasswordHasher<User>();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email?.Trim();

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Vui lòng nhập email và mật khẩu."
                );
                return View(model);
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .SingleOrDefaultAsync(u => u.Email == email);

            if (user == null || user.Role == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Email hoặc mật khẩu không đúng."
                );
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Tài khoản chưa có mật khẩu hợp lệ. Vui lòng đặt lại mật khẩu."
                );
                return View(model);
            }

            PasswordVerificationResult result;

            if (user.PasswordHash == model.Password)
{
    result = PasswordVerificationResult.Success;
}
else
{
    try
    {
        result = _hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            model.Password
        );
    }
    catch (FormatException)
    {
        result = PasswordVerificationResult.Failed;
    }
}
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Email hoặc mật khẩu không đúng."
                );
                return View(model);
            }

            if (result ==
                PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _hasher.HashPassword(
                    user,
                    model.Password
                );

                await _context.SaveChangesAsync();
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()
                ),
                new Claim(
                    ClaimTypes.Name,
                    user.FullName ?? string.Empty
                ),
                new Claim(
                    ClaimTypes.Email,
                    user.Email ?? string.Empty
                ),
                new Claim(
                    ClaimTypes.Role,
                    user.Role.RoleName
                )
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            switch (user.Role.RoleName.Trim().ToUpperInvariant())
            {
                case "ADMIN":
                    return RedirectToAction("Index", "Admin");

                case "EMPLOYER":
                    return RedirectToAction("Index", "Employer");

                case "CANDIDATE":
                    return RedirectToAction("Index", "Candidate");

                default:
                    await HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme
                    );
                    return RedirectToAction("AccessDenied");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
