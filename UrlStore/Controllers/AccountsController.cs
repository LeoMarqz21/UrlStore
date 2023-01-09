using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Claims;
using UrlStore.Helpers;
using UrlStore.Migrations;
using UrlStore.Models;
using UrlStore.Models.ViewModels;
using UrlStore.Services;

namespace UrlStore.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IEmailSender emailSender;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IFileStorage fileStorage;
        private readonly IWebHostEnvironment env;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AccountsController(
            ApplicationDbContext context, IMapper mapper, 
            UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender, RoleManager<IdentityRole> roleManager, IFileStorage fileStorage,
            IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor
            )
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            this.roleManager = roleManager;
            this.fileStorage = fileStorage;
            this.env = env;
            this.httpContextAccessor = httpContextAccessor;
        }

        /*==================================================*/
        /*============== Register and Login ================*/
        /*==================================================*/

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ViewData["returnUrl"] = returnUrl;
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ViewData["returnUrl"] = returnUrl;

            if(!ModelState.IsValid) return View(model);

            await CreateBasicRoles();

            var user = mapper.Map<User>(model);
            var result = await userManager.CreateAsync(user, user.PasswordHash);

            if(result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");

                var htmlMessage = $"<h3>Hola, Bienvenido a UrlStore</h3>" + 
                    $"<p>Gracias por registrarte con nosotros: <strong>{user.UserName}</strong></p>";

                await emailSender.SendEmailAsync(user.Email, "Bienvenida a UrlStore", htmlMessage);

                await signInManager.SignInAsync(user, isPersistent: true);

                ViewData["profile"] = await this.GetProfileImage();

                return LocalRedirect(returnUrl);
            }

            ErrorHandler(result);
            return View(model);
            
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ViewData["returnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ViewData["returnUrl"] = returnUrl;

            if (!ModelState.IsValid) return View(model);

            var result = await signInManager.PasswordSignInAsync(model.UserName, model.PasswordHash, isPersistent: model.RememberMe, lockoutOnFailure: true);

            if(result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Espere un minuto e intente iniciar sesión");
                return View(model);
            }

            if (result.Succeeded)
            {
                ViewData["profile"] = await this.GetProfileImage();
                return LocalRedirect(returnUrl);
            }
            
            ModelState.AddModelError(string.Empty, "Acceso denegado!!");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        /*==================================================*/
        /*================ External Access =================*/
        /*==================================================*/

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult ExternalAccess(string provider, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ViewData["returnUrl"] = returnUrl;
            var redirection = Url.Action(nameof(ExternalAccessCallback), "Accounts", new { ReturnUrl = returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirection);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalAccessCallback(string returnUrl = null, string error = null)
        {
            if(error is not null)
            {
                ModelState.AddModelError(string.Empty, $"Error en acceso externo: {error}");
                return View(nameof(Login));
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            
            if (info is null)
            {
                ModelState.AddModelError(string.Empty, "Algo salio mal, intentelo mas tarde");
                return RedirectToAction(nameof(Login));
            }

            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true);

            if(result.Succeeded)
            {
                await signInManager.UpdateExternalAuthenticationTokensAsync(externalLogin: info);
                return LocalRedirect(returnUrl);
            }else
            {
                ViewData["returnUrl"] = returnUrl;
                ViewData["providerName"] = info.ProviderDisplayName;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                return View("ConfirmExternalAccess", new ConfirmExternalAccessViewModel { Email = email, Name = name });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmExternalAccess(ConfirmExternalAccessViewModel model, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ViewData["returnUrl"] = returnUrl;

            if(!ModelState.IsValid)
            {
                return View(model);
            }

            await CreateBasicRoles();

            var info = await signInManager.GetExternalLoginInfoAsync();

            if(info == null) return View(model);

            var user = new User { Name = model.Name, UserName = model.UserName, Email = model.Email };

            var result = await userManager.CreateAsync(user);

            if(result.Succeeded)
            {
                result = await userManager.AddLoginAsync(user, info);

                if(result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: true);
                    await signInManager.UpdateExternalAuthenticationTokensAsync(info);

                    await userManager.AddToRoleAsync(user, "User");

                    var htmlMessage = $"<h3>Hola, Bienvenido a UrlStore</h3>" +
                    $"<p>Gracias por registrarte con nosotros: <strong>{user.UserName}</strong></p>";

                    await emailSender.SendEmailAsync(user.Email, "Bienvenida a UrlStore", htmlMessage);

                    return LocalRedirect(returnUrl);
                }
            }

            ErrorHandler(result);
            return View(model);
        }

        /*==================================================*/
        /*================ Profile =================*/
        /*==================================================*/

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.GetUserId();
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) return View(nameof(Error404));
            var profile = mapper.Map<ProfileViewModel>(user);
            return View(profile);
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile([FromForm] ProfileViewModel model)
        {
            if(!ModelState.IsValid) return View(model);

            var userId = User.GetUserId();
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            model.Email = user.Email;
            user = mapper.Map(model, user);
            if(model.Image != null)
            {
                using(var memoryStream = new MemoryStream())
                {
                    await model.Image.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(model.Image.FileName);
                    user.Photo = await fileStorage.Update(user.Photo, content, extension, "Profiles", model.Image.ContentType);
                }
            }

            await userManager.UpdateAsync(user);
            await signInManager.RefreshSignInAsync(user);

            return RedirectToAction(nameof(Profile));
        }

        private async Task<ActionResult<string>> GetProfileImage()
        {
            var userId = User.GetUserId();
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user.Photo is null)
            {
                var urlBase = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
                var urlImageDefault = Path.Combine(urlBase, "profiles", "profile.png").Replace("\\", "/");
                return urlImageDefault;
            }
            return user.Photo;
        }

        /*==================================================*/
        /*===================== Extras =====================*/
        /*==================================================*/

        [HttpGet]
        public IActionResult Error404()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Forbidden()
        {
            return View();
        }


        /*==================================================*/
        /*==================== Helpers =====================*/
        /*==================================================*/

        private void ErrorHandler(IdentityResult identityResult)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task CreateBasicRoles()
        {
            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
    }
}
