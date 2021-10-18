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
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Charges;
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

        public async Task<IReadOnlyCollection<ChargeLink>> CreateAsync(ChargeLinkCommandReceivedEvent chargeLinkEvent)
        {
            if (chargeLinkEvent == null) throw new ArgumentNullException(nameof(chargeLinkEvent));

            var chargeLinksCreated = new List<ChargeLink>();

            foreach (var chargeLinkCommand in chargeLinkEvent.ChargeLinkCommands)
            {
                var chargeLink = chargeLinkCommand.ChargeLink;

                var charge = await _chargeRepository
                    .GetChargeAsync(new ChargeSenderIdentifier(
                        chargeLink.SenderProvidedChargeId,
                        chargeLink.ChargeOwner,
                        chargeLink.ChargeType))
                    .ConfigureAwait(false);

                var meteringPoint = await _meteringPointRepository
                    .GetMeteringPointAsync(chargeLink.MeteringPointId)
                    .ConfigureAwait(false);

                var operation = new ChargeLinkOperation(
                    chargeLink.OperationId,
                    chargeLinkEvent.CorrelationId);
                var operations = new List<ChargeLinkOperation> { operation };

                var periodDetails = new ChargeLinkPeriodDetails(
                    chargeLink.StartDateTime,
                    chargeLink.EndDateTime.TimeOrEndDefault(),
                    chargeLink.Factor,
                    operation.Id);
                var periodDetailsCollection = new List<ChargeLinkPeriodDetails> { periodDetails };

                chargeLinksCreated.Add(new ChargeLink(
                    charge.Id,
                    meteringPoint.Id,
                    operations,
                    periodDetailsCollection));
            }

            return chargeLinksCreated;
        }
    }
}
