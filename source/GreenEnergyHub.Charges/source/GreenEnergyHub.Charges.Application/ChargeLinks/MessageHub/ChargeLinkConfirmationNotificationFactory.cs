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
using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub
{
    public class ChargeLinkConfirmationNotificationFactory : IAvailableDataNotificationFactory<AvailableChargeLinkReceiptData>
    {
        /// <summary>
        /// The upper anticipated weight (kilobytes) contribution to the final bundle of the charge link receipt.
        /// </summary>
        public const int MessageWeight = 2;

        /// <summary>
        /// Is used in communication with Message Hub.
        /// Be cautious to change it!
        /// </summary>
        public const string MessageTypePrefix = "ChargeLinkReceiptDataAvailable";

        public IReadOnlyList<DataAvailableNotificationDto> Create(IReadOnlyList<AvailableChargeLinkReceiptData> availableData)
        {
            return availableData.Select(
                    confirmation => new DataAvailableNotificationDto(
                        confirmation.AvailableDataReferenceId,
                        new GlobalLocationNumberDto(confirmation.RecipientId),
                        new MessageTypeDto(MessageTypePrefix + "_" + confirmation.BusinessReasonCode),
                        DomainOrigin.Charges,
                        true,
                        MessageWeight))
                .ToList();
        }
    }
}
