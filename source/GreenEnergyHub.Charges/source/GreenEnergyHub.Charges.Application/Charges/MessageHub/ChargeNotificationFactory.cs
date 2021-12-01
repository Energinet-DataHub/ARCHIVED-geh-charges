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
using System.Linq;
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.Application.MessageHub;
using GreenEnergyHub.Charges.Domain.AvailableChargeData;

namespace GreenEnergyHub.Charges.Application.Charges.MessageHub
{
    public class ChargeNotificationFactory : IAvailableDataNotificationFactory<AvailableChargeData>
    {
        /// <summary>
        /// The upper anticipated weight (kilobytes) contribution to the final bundle from the charge created event.
        /// </summary>
        public const decimal ChargeMessageWeight = 5m;
        public const decimal ChargePointMessageWeight = 0.2m;
        public const string MessageTypePrefix = "ChargeDataAvailable";

        public IReadOnlyList<DataAvailableNotificationDto> Create(IReadOnlyList<AvailableChargeData> availableData)
        {
            return availableData.Select(
                a => new DataAvailableNotificationDto(
                    a.AvailableDataReferenceId,
                    new GlobalLocationNumberDto(a.RecipientId),
                    new MessageTypeDto(MessageTypePrefix + "_" + a.BusinessReasonCode),
                    DomainOrigin.Charges,
                    SupportsBundling: true,
                    GetMessageWeight(a)))
                .ToList();
        }

        private int GetMessageWeight(AvailableChargeData chargeData)
        {
            return (int)Math.Round(
                (chargeData.Points.Count * ChargePointMessageWeight) + ChargeMessageWeight,
                MidpointRounding.AwayFromZero);
        }
    }
}
