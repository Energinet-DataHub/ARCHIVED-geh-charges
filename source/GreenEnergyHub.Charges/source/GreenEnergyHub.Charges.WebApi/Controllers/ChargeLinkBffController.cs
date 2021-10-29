using System.Collections.Generic;
using System.Linq;
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.QueryApi.ScaffoldedModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChargeLinkBffController : ControllerBase
    {
        private readonly ILogger<ChargeLinkBffController> _logger;
        private readonly IData _data;

        public ChargeLinkBffController(ILogger<ChargeLinkBffController> logger, IData data)
        {
            _logger = logger;
            _data = data;
        }

        [HttpGet]
        public IEnumerable<object> Get(string meteringPointId)
        {
            _logger.LogCritical("Hey BFF!");

            return _data
                .ChargeLinks
                .ForMeteringPoint(meteringPointId)
                .Select(c => new
                {
                    ChargeId = c.Charge.SenderProvidedChargeId,
                    ChargeType = c.Charge.ChargeType,
                    ChargeOwner = c.Charge.MarketParticipant,
                })
                .ToList();
        }


    }
}
