using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Out_of_Office.Infrastructure.Identity;
using Out_of_Office.Models;
using System.Text.Encodings.Web;
using Out_of_Office.Application.Common.Interfaces;
namespace Out_of_Office.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Login() => View();

       
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                return View("ForgotPasswordConfirmation"); // nie zdradzamy czy konto istnieje

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account",
                new { email = user.Email, token },
                protocol: Request.Scheme, host: Request.Host.Value);

            var body = $@"
        <p>To reset your password click:</p>
        <p><a href=""{HtmlEncoder.Default.Encode(callbackUrl!)}"">Reset password</a></p>";

            await _emailSender.SendAsync(user.Email!, "Reset your password", body);
            return View("ForgotPasswordConfirmation");
        }

        // === RESET PASSWORD ===
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token)) return BadRequest();
            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return RedirectToAction(nameof(ResetPasswordConfirmation));

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                return View(model);
            }
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation() => View();

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation() => View();
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

            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var verificationCode = model.Code?.Replace(" ", "").Replace("-", "");
            if (string.IsNullOrEmpty(verificationCode))
            {
                ModelState.AddModelError(nameof(model.Code), "The verification code is required. Please enter the code from your authenticator app.");
                return View(new EnableAuthenticatorViewModel { Key = key }); // Ensure key is passed again
            }

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError(nameof(model.Code), "The verification code is incorrect. Please try again with the correct code from your authenticator app.");
                return View(new EnableAuthenticatorViewModel { Key = key }); // Ensure key is passed again
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);

            TempData["SuccessMessage"] = "Two-factor authentication has been successfully enabled.";

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            TempData["RecoveryCodes"] = recoveryCodes;

            return RedirectToAction("EmployeeProfile", "Employee");
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
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ResetAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);

            TempData["SuccessMessage"] = "Authenticator reset. Please configure a new authenticator app.";
            return RedirectToAction("EnableAuthenticator");
        }

    }

}
