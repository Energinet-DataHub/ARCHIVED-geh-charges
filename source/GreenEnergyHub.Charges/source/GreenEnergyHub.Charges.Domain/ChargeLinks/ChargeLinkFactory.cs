// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Domain.MeteringPoints;

namespace GreenEnergyHub.Charges.Domain.ChargeLinks
{
    public class ChargeLinkFactory : IChargeLinkFactory
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IMeteringPointRepository _meteringPointRepository;

        public ChargeLinkFactory(IChargeRepository chargeRepository, IMeteringPointRepository meteringPointRepository)
        {
            _chargeRepository = chargeRepository;
            _meteringPointRepository = meteringPointRepository;
        }

        public async Task<IReadOnlyCollection<ChargeLink>> CreateAsync(ChargeLinksReceivedEvent chargeLinksEvent)
        {
            if (chargeLinksEvent == null) throw new ArgumentNullException(nameof(chargeLinksEvent));

            var chargeLinksCreated = new List<ChargeLink>();

            foreach (var chargeLink in chargeLinksEvent.ChargeLinksCommand.ChargeLinks)
            {
                var chargeIdentifier = new ChargeIdentifier(chargeLink.SenderProvidedChargeId, chargeLink.ChargeOwner, chargeLink.ChargeType);
                var charge = await _chargeRepository.GetAsync(chargeIdentifier).ConfigureAwait(false);

                var meteringPoint = await _meteringPointRepository
                    .GetMeteringPointAsync(chargeLinksEvent.ChargeLinksCommand.MeteringPointId)
                    .ConfigureAwait(false);

                chargeLinksCreated.Add(new ChargeLink(
                    charge.Id,
                    meteringPoint.Id,
                    chargeLink.StartDateTime,
                    chargeLink.EndDateTime.TimeOrEndDefault(),
                    chargeLink.Factor));
            }

            return chargeLinksCreated;
        }
    }
}
