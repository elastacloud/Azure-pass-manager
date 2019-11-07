using APM.Domain;
using System.Threading.Tasks;

namespace APM.BotCore
{
    public interface IAPMHelper
    {
        Task<Code> GetAzurePassCode(string userResponse);
    }
}