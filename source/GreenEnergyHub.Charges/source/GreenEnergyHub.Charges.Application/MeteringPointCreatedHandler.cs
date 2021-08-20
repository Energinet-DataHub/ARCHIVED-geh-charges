using System;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Domain.Events.Integration;

namespace GreenEnergyHub.Charges.Application
{
    public class MeteringPointCreatedHandler : IMeteringPointCreatedHandler
    {
        private readonly IMeteringPointRepository _meteringPointRepository;

        public MeteringPointCreatedHandler(IMeteringPointRepository meteringPointRepository)
        {
            _meteringPointRepository = meteringPointRepository;
        }

        public async Task HandleAsync(MeteringPointCreatedEvent meteringPointCreatedEvent)
        {
            if (meteringPointCreatedEvent == null)
            {
                throw new ArgumentNullException(nameof(meteringPointCreatedEvent));
            }

            await _meteringPointRepository.StoreMeteringPointAsync(meteringPointCreatedEvent).ConfigureAwait(false);
        }
    }
}
