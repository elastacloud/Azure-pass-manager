namespace APM.BotCore
{
    using APM.Domain;
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class APMHelper : IAPMHelper
    {
        private readonly Uri apmEndPoint;

        public APMHelper(Uri apmEndPoint)
        {
            this.apmEndPoint = apmEndPoint;
        }
        public async Task<Code> GetAzurePassCode(string userResponse)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var claimUri = new Uri(apmEndPoint, $"claim?eventName={userResponse}");
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, claimUri);

                Debug.WriteLine($"Hitting: {apmEndPoint}");
                using (HttpResponseMessage response = await client.SendAsync(httpRequest))
                {
                    response.EnsureSuccessStatusCode();
                    string resp = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Code>(resp);
                }
            }
        }
    }
}