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
using GreenEnergyHub.Charges.MessageHub.BundleSpecification;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.MessageHub
{
    public class AvailableDataNotificationFactory<TAvailableData> : IAvailableDataNotificationFactory<TAvailableData>
        where TAvailableData : AvailableDataBase
    {
        public IReadOnlyList<DataAvailableNotificationDto> Create(
            IReadOnlyList<TAvailableData> availableData,
            IBundleSpecification<TAvailableData> bundleSpecification)
        {
            return availableData.Select(
                    data => new DataAvailableNotificationDto(
                        data.AvailableDataReferenceId,
                        new GlobalLocationNumberDto(data.RecipientId),
                        new MessageTypeDto(bundleSpecification.GetMessageType(data.BusinessReasonCode)),
                        DomainOrigin.Charges,
                        true,
                        bundleSpecification.GetMessageWeight(data),
                        data.DocumentType.ToString()))
                .ToList();
        }
    }
}
