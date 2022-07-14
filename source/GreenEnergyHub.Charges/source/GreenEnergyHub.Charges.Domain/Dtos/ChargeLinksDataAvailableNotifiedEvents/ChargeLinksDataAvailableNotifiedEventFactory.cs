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

using System.Linq;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksDataAvailableNotifiedEvents
{
    public class ChargeLinksDataAvailableNotifiedEventFactory : IChargeLinksDataAvailableNotifiedEventFactory
    {
        public ChargeLinksDataAvailableNotifiedEvent Create(ChargeLinksAcceptedEvent chargeLinksAcceptedEvent)
        {
            var meteringPointId = GetMeteringPointIdFromFirstChargeLinkDto(chargeLinksAcceptedEvent);
            return new ChargeLinksDataAvailableNotifiedEvent(chargeLinksAcceptedEvent.PublishedTime, meteringPointId);
        }

        /// <summary>
        /// Charge links received through ChargeLinksIngestion will only contain a single
        /// <see cref="ChargeLinkDto"/> and for charge links created from default charge links we only
        /// notify once, hence we can get meteringPointId from the first <see cref="ChargeLinkDto"/>.
        /// </summary>
        /// <param name="chargeLinksAcceptedEvent"></param>
        private static string GetMeteringPointIdFromFirstChargeLinkDto(ChargeLinksAcceptedEvent chargeLinksAcceptedEvent)
        {
            return chargeLinksAcceptedEvent.ChargeLinksCommand.Operations.First().MeteringPointId;
        }
    }
}
