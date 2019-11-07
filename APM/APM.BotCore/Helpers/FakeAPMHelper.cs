using APM.Domain;
using System;
using System.Threading.Tasks;

namespace APM.BotCore
{
    public class FakeAPMHelper : IAPMHelper
    {
        public async Task<Code> GetAzurePassCode(string userResponse)
        {
            return await Task.Run(() =>
            {
                return new Code
                {
                    AvaliableFrom = DateTime.UtcNow.AddDays(-7d),
                    AvaliableUntil = DateTime.UtcNow.AddDays(7d),
                    Claimed = false,
                    EventName = userResponse,
                    Expiry = DateTime.UtcNow.AddDays(14d),
                    Owner = "Fake",
                    PromoCode = "abc123abc"
                };
            });
        }
    }
}