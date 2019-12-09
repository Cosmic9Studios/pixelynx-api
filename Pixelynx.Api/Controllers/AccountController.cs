using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pixelynx.Api.Helpers;
using Pixelynx.Api.Requests;
using Pixelynx.Api.Responses;
using Pixelynx.Api.Settings;
using Pixelynx.Data.Entities;
using Pixelynx.Logic.Interfaces;

namespace Pixelynx.Api.Controllers
{
    [Route("account")]
    public class AccountController : ControllerBase
    {
        private ILogger<AccountController> logger;
        private UserManager<UserEntity> userManager;

        #region Constructors
        public AccountController(ILogger<AccountController> logger, UserManager<UserEntity> userManager)
        {
            this.logger = logger;
            this.userManager = userManager;
        }
        #endregion

        #region Enums
        public enum ConfirmationType 
        {
            Account, 
            ResetPassword
        }
        #endregion

        #region Public Methods
        [HttpGet, Route("me")]
        public async Task<ActionResult<LoginResponse>> GetUserData(LoginRequest request)
        {
            var email = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest();
            }

            return Ok(new UserDataResponse
            {
                UserName = user.UserName,
                FirstName = user.FirstName, 
                LastName = user.LastName
            });
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
            if (!user.EmailConfirmed) 
            {
                return Unauthorized();
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (result.Succeeded)
            {
                await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, user.Email));
                var token = await user.GenerateToken(userManager, authSettings.Value.JWTSecret);
                return new LoginResponse 
                {
                    Token = token
                };
            }

            return BadRequest("Invalid username or password.");
        }

        [HttpPost, Route("logout")]
        public async Task<IActionResult> Logout([FromServices] SignInManager<UserEntity> signInManager)
        {
            logger.LogInformation($"Logout {User.Identity.Name}");
            await signInManager.SignOutAsync();
            return Ok();
        }

        [HttpPost, Route("register")]
        public async Task<IActionResult> Register([FromServices] IEmailService emailService, [FromBody] RegistrationRequest request)
        {
            IdentityResult result = null;
            var user = new UserEntity 
            { 
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            try 
            {
                result = await userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    await SendRegistrationEmail(emailService, user);
                    return NoContent();
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
            }
            
            return BadRequest(result?.Errors);
        }

        [HttpPost, Route("resendemail")]
        public async Task<IActionResult> ResendEmail([FromServices] IEmailService emailService, [FromQuery] string userId = "", string email = "")
        {
            UserEntity user = null;
            if (!string.IsNullOrEmpty(userId))
            {
                user = await userManager.FindByIdAsync(userId);
            }
            else if (!string.IsNullOrEmpty(email))
            {
                user = await userManager.FindByEmailAsync(email);
            }

            if (user != null)
            {
                await SendRegistrationEmail(emailService, user); 
            }

            return Ok();
        }

        [HttpGet, Route("confirm")]
        public async Task<IActionResult> ConfirmAccount([FromServices] IEmailService emailService, [FromQuery] string userId, [FromQuery] string code, string type)
        {
            var errors = new List<string>();
            if (Enum.TryParse<ConfirmationType>(type, true, out var confirmationType))
            {
                var user = await userManager.FindByIdAsync(userId);
                if (confirmationType == ConfirmationType.Account)
                {
                    var result = await userManager.ConfirmEmailAsync(user, code);  
                    if (result.Succeeded)
                    {
                        await userManager.AddClaimAsync(user, new Claim("scope", "assets:read assets:write"));
                    }
                    else 
                    {
                        errors.AddRange(result.Errors.Select(x => x.Description));
                    }
                }
                else if (confirmationType == ConfirmationType.ResetPassword)
                {
                    var result = await userManager.VerifyUserTokenAsync(user, userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", code);
                    if (!result)
                    {
                        errors.Add("Invalid password reset token");
                    }
                }
            }

            if (errors.Any())
            {
                return BadRequest(errors);
            }
            
            return Ok();
        }

        [HttpPost, Route("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromServices] IEmailService emailService, [FromQuery] string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            var code = HttpUtility.UrlEncode(await userManager.GeneratePasswordResetTokenAsync(user));
            var confirmationUrl = user.GenerateConfirmationUrl(this.Request, code, ConfirmationType.ResetPassword);

            emailService.SendEmailFromTemplate(user.Email, "Pixelynx - Forgot Password", "ForgotPassword", new Dictionary<string, string> 
            {
                ["Sender"] = user.FirstName,
                ["Button_Url"] = confirmationUrl
            });

            return Ok();
        }

        [HttpPost, Route("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            var result = await userManager.ResetPasswordAsync(user, HttpUtility.UrlDecode(request.Code), request.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }
        #endregion

        #region Private Methods
        private async Task SendRegistrationEmail(IEmailService emailService, UserEntity user)
        {
            var code = HttpUtility.UrlEncode(await userManager.GenerateEmailConfirmationTokenAsync(user));
            var confirmationUrl = user.GenerateConfirmationUrl(this.Request, code, ConfirmationType.Account);
            emailService.SendEmailFromTemplate(user.Email, "Pixelynx - Confirm your account", "Register", new Dictionary<string, string> 
            {
                ["Sender"] = user.FirstName,
                ["Button_Url"] = confirmationUrl
            });
        }
        #endregion
    }
}