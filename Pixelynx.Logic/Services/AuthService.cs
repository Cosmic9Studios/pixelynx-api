using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic;
using Pixelynx.Logic.Extensions;
using Pixelynx.Logic.Interfaces;
using Pixelynx.Logic.Model.Email;
using Pixelynx.Logic.Models;
using Pixelynx.Logic.Services;
using Stripe;

public class AuthService : IAuthService
{
    private ILogger<AuthService> logger;
    private UserManager<UserEntity> userManager;
    private SignInManager<UserEntity> signInManager;
    private IEmailService emailService;

    public enum ConfirmationType 
    {
        Account, 
        ResetPassword
    };
    
    public AuthService(
        ILogger<AuthService> logger, 
        UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager,
        IEmailService emailService)
    {
        this.logger = logger;
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.emailService = emailService;
    }

    public async Task<string> Login(string email, string password, string jwtSecret)
    {
        logger.LogInformation($"Attempting login for {email}");

        var user = await userManager.FindByEmailAsync(email);
        if (user == null || !user.EmailConfirmed) 
        {
            user = await userManager.FindByNameAsync(email);
            if (user == null || !user.EmailConfirmed) 
            {
                return "NotConfirmed";
            }
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
        if (result.Succeeded)
        {
            await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, user.Email));
            var token = await user.GenerateToken(userManager, jwtSecret);
            return token;
        }

        return null;
    }

    public async Task<bool> Logout() 
    {
        // await signInManager.SignOutAsync();
        return true;
    }

    public async Task<GenericResult<string>> Register(HttpRequest request, UserEntity newUser, string password)
    {
        IdentityResult result = null;

        try 
        {
            result = await userManager.CreateAsync(newUser, password);
            if (result.Succeeded)
            {
                await SendRegistrationEmail(emailService, request, newUser);
                return new GenericResult<string>
                {
                    Succeeded = true,
                };
            }
        }
        catch(Exception ex)
        {
            logger.LogError(ex.Message);
        }
        
        return new GenericResult<string>
        {
            Succeeded = false,
            Errors = result?.Errors.Select(x => x.Description).ToList()
        };
    }

    public async Task<bool> ForgotPassword(HttpRequest request, string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        var code = HttpUtility.UrlEncode(await userManager.GeneratePasswordResetTokenAsync(user));
        var confirmationUrl = user.GenerateConfirmationUrl(request, code, ConfirmationType.ResetPassword);

        await emailService.SendEmailFromTemplateAsync(EmailTemplate.ForgotPassword, user.Email, "Pixelynx - Forgot Password", new RegistrationData
        {
            Receipient = user.FirstName,
            ButtonUrl = confirmationUrl
        });

        return true;
    }

    public async Task<GenericResult<string>> ResetPassword(string userId, string code, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId);
        var result = await userManager.ResetPasswordAsync(user, HttpUtility.UrlDecode(code), newPassword);

        if (!result.Succeeded)
        {
            return new GenericResult<string>
            {
                Succeeded = false,
                Errors = result.Errors.Select(x => x.Description).ToList()
            };
        }

        return new GenericResult<string>
        {
            Succeeded = true,
        };
    }

    public async Task<GenericResult<string>> ConfirmEmail(IDbContextFactory dbContextFactory,
         string userId, string code, string type)
    {
        var errors = new List<string>();
        if (Enum.TryParse<ConfirmationType>(type, true, out var confirmationType))
        {
            var user = await userManager.FindByIdAsync(userId);
            if (confirmationType == ConfirmationType.Account)
            {
                var result = await userManager.ConfirmEmailAsync(user, HttpUtility.UrlDecode(code));  
                if (result.Succeeded)
                {
                    var options = new CustomerCreateOptions();
                    var service = new CustomerService();
                    var customer = service.Create(options);

                    using (var context = dbContextFactory.CreateWrite())
                    {
                        await context.PaymentDetails.AddAsync(new PaymentDetailsEntity
                        {
                            UserId = user.Id, 
                            CustomerId = customer.Id
                        });

                        await context.SaveChangesAsync();
                    }
                }
                else 
                {
                    errors.AddRange(result.Errors.Select(x => x.Description));
                }
            }
            else if (confirmationType == ConfirmationType.ResetPassword)
            {
                var result = await userManager.VerifyUserTokenAsync(user, userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", HttpUtility.UrlDecode(code));
                if (!result)
                {
                    errors.Add("Invalid password reset token");
                }
            }
        }

        if (errors.Any())
        {
            return new GenericResult<string>
            {
                Succeeded = false,
                Errors = errors
            };
        }
        else 
        {
            return new GenericResult<string>
            {
                Succeeded = true
            };
        }
    }

    public async Task<bool> ResendEmail(HttpRequest request, string userId, string email)
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
            await SendRegistrationEmail(emailService, request, user);
            return true;
        }

        return false;
    }

    public async Task<GenericResult<string>> UpdatePassword(string userId, string oldPassword, string newPassword)
    {
        UserEntity user = null;
        if (!string.IsNullOrEmpty(userId))
        {
            user = await userManager.FindByIdAsync(userId);
        }

        var errors = new List<string>();
        if (!string.IsNullOrWhiteSpace(oldPassword) && !string.IsNullOrWhiteSpace(oldPassword))
        {
            var result = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors.Select(x => x.Description));
            }
        }
        else
        {
            errors.Add("Field must not be empty");
        }

        return new GenericResult<string>
        {
            Succeeded = !errors.Any(),
            Errors = errors
        };
    }

    #region Private Methods
    private async Task SendRegistrationEmail(IEmailService emailService, HttpRequest request, UserEntity user)
    {
        var code = HttpUtility.UrlEncode(await userManager.GenerateEmailConfirmationTokenAsync(user));
        var confirmationUrl = user.GenerateConfirmationUrl(request, code, ConfirmationType.Account);
        await emailService.SendEmailFromTemplateAsync(EmailTemplate.Registration, user.Email, "Pixelynx - Confirm your account", new RegistrationData
        {
            Receipient = user.FirstName,
            ButtonUrl = confirmationUrl
        });
    }
    #endregion
}