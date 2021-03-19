using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace GreenEnergyHub.Charges.MessageReceiver
{
    public static class ChargeHttpTrigger
    {
        [FunctionName(nameof(ChargeHttpTrigger))]
        public static Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            [NotNull] HttpRequest req)
        {
            return Task.FromResult((IActionResult)new OkObjectResult(req.Body));
        }
    }
}
