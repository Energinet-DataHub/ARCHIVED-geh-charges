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

using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.Application.MessageHub;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using Microsoft.Extensions.Azure;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub
{
    public class ChargeLinkNotificationFactory : IAvailableDataNotificationFactory<AvailableChargeLinksData>
    {
        /// <summary>
        /// The upper anticipated weight (kilobytes) contribution to the final bundle from the charge link created event.
        /// </summary>
        public const int MessageWeight = 2;

        /// <summary>
        /// Is used in communication with Message Hub.
        /// Be cautious to change it!
        /// </summary>
        public const string MessageTypePrefix = "ChargeLinkDataAvailable";

        public IReadOnlyList<DataAvailableNotificationDto> Create(IReadOnlyList<AvailableChargeLinksData> availableData)
        {
            return availableData.Select(
                link => new DataAvailableNotificationDto(
                    link.AvailableDataReferenceId,
                    new GlobalLocationNumberDto(link.RecipientId),
                    new MessageTypeDto(MessageTypePrefix + "_" + link.BusinessReasonCode),
                    DomainOrigin.Charges,
                    true,
                    MessageWeight))
                .ToList();
        }
    }
}
