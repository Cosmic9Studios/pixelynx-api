using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pixelynx.Api.Models;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic.Models;
using Stripe;

namespace Pixelynx.Api.Controllers
{
    [Route("admin"), Authorize]
    public class AdminController : Controller
    {
        private readonly IVaultService vaultService;

        public AdminController(IVaultService vaultService)
        {
            this.vaultService = vaultService;
        }

        private async Task<bool> IsAdmin(Guid userId)
        {
            var admins = (await vaultService.GetAuthSecrets()).Admins;
            if (admins != null)
            {
                return admins.Split(";")
                    .Any(x => Guid.Parse(x) == userId);
            }

            return false;
        }
        
        
        [Route("transactions"), HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] int month, [FromServices] IDbContextFactory dbContextFactory)
        {
            var userId = Guid.Parse(HttpContext.User.Identity.Name);
            var isAdmin = await IsAdmin(userId);
            if (!isAdmin)
            {
                return Unauthorized();
            }
            
            var nextMonth = month + 1;
            var year = DateTime.UtcNow.Year;

            if (nextMonth == 13)
            {
                nextMonth = 1;
                year += 1;
            }
            
            var options = new ChargeListOptions
            {
                Created = new DateRangeOptions
                {
                    GreaterThanOrEqual = DateTime.Parse($"{DateTime.UtcNow.Year}-{month.ToString().PadLeft(2, '0')}-01"),
                    LessThan = DateTime.Parse($"{year}-{nextMonth.ToString().PadLeft(2, '0')}-01")
                }
            };
            
            var service = new ChargeService();
            using (var dbContext = dbContextFactory.CreateRead())
            {
                var transactionList = service.ListAutoPaging(options)
                    .Where(x => x.Metadata["assets"] != null)
                    .SelectMany(x =>
                    {
                        var assets = JsonConvert.DeserializeObject<List<AssetMetadata>>(x.Metadata["assets"]);
                        return assets.Select(asset => new AssetTransaction
                        {
                            OwnerId = asset.OwnerId,
                            OwnerName = dbContext.Users.FirstOrDefault(user => user.Id == asset.OwnerId)?.UserName,
                            TransactionId = x.Id,
                            Cost = asset.Cost,
                            TransactionTotal = (int) x.Amount / 100
                        });
                    }).ToList();
                return Ok(transactionList);
            }
        }
    }
}