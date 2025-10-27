using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Out_of_Office.Infrastructure.Identity;
using Out_of_Office.Models;
using System.Text.Encodings.Web;
using Out_of_Office.Application.Common.Interfaces;
using Out_of_Office;
using Microsoft.Extensions.Localization;
namespace Out_of_Office.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IStringLocalizer _stringLocalizer;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IEmailSender emailSender, IStringLocalizerFactory factory)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            var asm = typeof(Program).Assembly.GetName().Name!;
            _stringLocalizer = factory.Create("Views.Account.Login", asm);
        }
        [HttpGet]
        public IActionResult TestLoc([FromServices] IStringLocalizer<SharedResource> L)
        {
            // 1) Wypisz wartość klucza
            var value = L["InvalidCredentials"].Value;

            // 2) Policz, czy w ogóle widzi jakieś stringi
            var all = L.GetAllStrings(includeParentCultures: true).ToList();
            var count = all.Count;

            return Content($"Value='{value}', Count={count}");
        }
        [HttpGet]
        public IActionResult ListRes()
        {
            var names = typeof(Program).Assembly.GetManifestResourceNames();
            return Content(string.Join("\n", names));
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

            var subject = _stringLocalizer["ResetPasswordEmailSubject"];
            var body = string.Format(
                _stringLocalizer["ResetPasswordEmailBodyHtml"],
                HtmlEncoder.Default.Encode(callbackUrl!)
            );
            await _emailSender.SendAsync(user.Email!, subject, body);
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

            ModelState.AddModelError(string.Empty, _stringLocalizer["InvalidCredentials"]);
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
                TempData["ErrorMessage"] = _stringLocalizer["PasswordsDoNotMatch"];
                return RedirectToAction("EmployeeProfile", "Employee");
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction("EmployeeProfile", "Employee");
            }

            TempData["SuccessMessage"] = _stringLocalizer["PasswordChangedSuccessfully"];
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
                ModelState.AddModelError(nameof(model.Code), _stringLocalizer["VerificationCodeRequired"]);
                return View(new EnableAuthenticatorViewModel { Key = key }); // Ensure key is passed again
            }

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError(nameof(model.Code), _stringLocalizer["VerificationCodeIncorrect"]);
                return View(new EnableAuthenticatorViewModel { Key = key }); // Ensure key is passed again
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);

            TempData["SuccessMessage"] = _stringLocalizer["TwoFactorEnabledSuccess"];

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
                ModelState.AddModelError(string.Empty, _stringLocalizer["SessionExpired"]);
                return RedirectToAction("Login");
            }

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, false, false);

            if (result.Succeeded)
            {
                HttpContext.Session.Remove("2FAUser");
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, _stringLocalizer["Invalid2faCode"]);
            return View(model);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ResetAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);

            TempData["SuccessMessage"] = _stringLocalizer["AuthenticatorResetSuccess"];
            return RedirectToAction("EnableAuthenticator");
        }

    }

}
