using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pixelynx.Api.Helpers;
using Pixelynx.Api.Requests;
using Pixelynx.Api.Responses;
using Pixelynx.Api.Settings;
using Pixelynx.Data.Entities;
using Pixelynx.Logic.Interfaces;

namespace Pixelynx.Api.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private ILogger<AccountController> logger;
        private UserManager<UserEntity> userManager;

        public AccountController(ILogger<AccountController> logger, UserManager<UserEntity> userManager)
        {
            this.logger = logger;
            this.userManager = userManager;
        }

        [HttpPost, Route("login"), AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login(
            [FromServices] SignInManager<UserEntity> signInManager,
            [FromServices] IOptions<AuthSettings> authSettings,
            [FromBody] LoginRequest request 
        )
        {
            logger.LogInformation($"Attempting login for {request.Email}");

            var user = await userManager.FindByEmailAsync(request.Email);
            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (result.Succeeded)
            {
                var token = await user.GenerateToken(userManager, authSettings.Value.JWTSecret);
                return new LoginResponse 
                {
                    Id = user.Id, 
                    Token = token, 
                    Email = user.Email, 
                    UserName = user.UserName
                };
            }

            return BadRequest("Invalid username or password.");
        }

        [HttpPost, Route("logout")]
        public async Task<IActionResult> Logout([FromServices] SignInManager<UserEntity> signInManager)
        {
            logger.LogInformation($"Logout {User.Identity.Name}");
            await signInManager.SignOutAsync();
            return NoContent();
        }

        [HttpPost, Route("register")]
        public async Task<IActionResult> Register([FromServices] IEmailService emailService, [FromBody] LoginRequest request)
        {
            var user = new UserEntity { UserName = request.Email, Email = request.Email };
            var result = await userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                await SendRegistrationEmail(emailService, user, request);
                return NoContent();
            }

            return BadRequest(new { Message = "Unable to create user", Errors = result.Errors });
        }

        [HttpPost, Route("resendemail")]
        public async Task<IActionResult> ResendEmail([FromServices] IEmailService emailService, [FromBody] LoginRequest request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            await SendRegistrationEmail(emailService, user, request);

            return NoContent();
        }

        [HttpGet, Route("confirmemail")]
        public async Task<IActionResult> ConfirmEmail([FromServices] IEmailService emailService, string userId, string code)
        {
            var user = await userManager.FindByIdAsync(userId);
            var result = await userManager.ConfirmEmailAsync(user, code);
            
            if (result.Succeeded)
            {
                await userManager.AddClaimAsync(user, new Claim("scope", "assets:read assets:write"));
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        #region Private Methods. 
        private async Task SendRegistrationEmail(IEmailService emailService, UserEntity user, LoginRequest request)
        {
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var baseUrl = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}";
            var callbackUrl = Url.Action(
                "ConfirmEmail", "Account", 
                new { userId = user.Id, code = code }
            );

            emailService.SendEmail(request.Email, 
                "Confirm your account", 
                $"Please confirm your account by clicking this link: <a href=\"{baseUrl}{callbackUrl}\">link</a>"
            );
        }
        #endregion
    }
}