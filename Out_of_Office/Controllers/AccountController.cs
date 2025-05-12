using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Out_of_Office.Infrastructure.Identity;
using Out_of_Office.Models;

namespace Out_of_Office.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            if (result.RequiresTwoFactor)
            {
                HttpContext.Session.SetString("2FAUser", model.Username);
                return RedirectToAction("LoginWith2fa");
            }
            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match.";
                return RedirectToAction("EmployeeProfile", "Employee");
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction("EmployeeProfile", "Employee");
            }

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("EmployeeProfile", "Employee");
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            var key = await _userManager.GetAuthenticatorKeyAsync(user);

            var model = new EnableAuthenticatorViewModel { Key = key };
            return View(model);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var verificationCode = model.Code.Replace(" ", "").Replace("-", "");

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("", "Incorrect verification code.");
                return View(model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);

            TempData["SuccessMessage"] = "Two-factor authentication has been successfully enabled.";

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            TempData["RecoveryCodes"] = recoveryCodes;

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult LoginWith2fa() => View();

        [HttpPost]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var username = HttpContext.Session.GetString("2FAUser");
            if (username == null)
            {
                ModelState.AddModelError("", "Session expired. Try logging in again.");
                return RedirectToAction("Login");
            }

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, false, false);

            if (result.Succeeded)
            {
                HttpContext.Session.Remove("2FAUser");
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid authenticator code.");
            return View(model);
        }
    }
}
