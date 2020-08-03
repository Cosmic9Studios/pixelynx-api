using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic.Models;
using Stripe;

namespace Pixelynx.Logic.Services
{
    public class PayoutService
    {
        private readonly IDbContextFactory dbContextFactory;
        public PayoutService(IDbContextFactory dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }
        
        public async Task<UserBalance> GetUserBalance(Guid userId)
        {
            var currentDay = DateTime.UtcNow.Day;
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            var startMonth = currentDay > 15 ? currentMonth : currentMonth - 1;
            var startYear = currentYear;
            if (startMonth == 0)
            {
                startMonth = 12;
                startYear -= 1;
            }
            
            var options = new ChargeListOptions
            {
                Created = new DateRangeOptions
                {
                    GreaterThanOrEqual = DateTime.Parse($"{startYear}-{startMonth.ToString().PadLeft(2, '0')}-01"),
                    LessThanOrEqual = DateTime.Parse($"{currentYear}-{currentMonth.ToString().PadLeft(2, '0')}-{currentDay}")
                }
            };
            
            var service = new ChargeService();
            var userCharges = service.ListAutoPaging(options)
                .Where(x => x.Metadata["assets"] != null)
                .SelectMany(x =>
                {
                    var assets = JArray.Parse(x.Metadata["assets"]);
                    return assets.Select(asset => new
                    {
                        OwnerId = Guid.Parse(assets[0]["OwnerId"].ToString()),
                        Date = x.Created,
                        Cost = asset["Cost"].Value<float>(),
                    });
                })
                .Where(x => x.OwnerId == userId)
                .ToList();

            var userBalance = new UserBalance
            {
                TotalBalance = userCharges.Sum(x => x.Cost),
                NextPayoutBalance = userCharges.Where(x => x.Date.Month == startMonth)
                    .Sum(x => x.Cost)
            };

            return userBalance;
        }
    }
}