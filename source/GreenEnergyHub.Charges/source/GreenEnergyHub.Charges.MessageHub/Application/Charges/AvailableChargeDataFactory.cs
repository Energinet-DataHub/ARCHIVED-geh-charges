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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Application.Charges
{
    public class AvailableChargeDataFactory : IAvailableDataFactory<AvailableChargeData, ChargeCommandAcceptedEvent>
    {
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IMessageMetaDataContext _messageMetaDataContext;

        public AvailableChargeDataFactory(
            IMarketParticipantRepository marketParticipantRepository,
            IMessageMetaDataContext messageMetaDataContext)
        {
            _marketParticipantRepository = marketParticipantRepository;
            _messageMetaDataContext = messageMetaDataContext;
        }

        public async Task<IReadOnlyList<AvailableChargeData>> CreateAsync(ChargeCommandAcceptedEvent input)
        {
            var result = new List<AvailableChargeData>();

            if (ShouldMakeDataAvailableForActiveGridProviders(input))
            {
                var activeGridAccessProviders = await _marketParticipantRepository.GetActiveGridAccessProvidersAsync();
                var operation = input.Command.ChargeOperation;

                foreach (var provider in activeGridAccessProviders)
                {
                    var points =
                        operation.Points
                            .Select(x => new AvailableChargeDataPoint(x.Position, x.Price))
                            .ToList();

                    result.Add(new AvailableChargeData(
                        provider.MarketParticipantId,
                        provider.BusinessProcessRole,
                        input.Command.Document.BusinessReasonCode,
                        _messageMetaDataContext.RequestDataTime,
                        Guid.NewGuid(), // ID of each available piece of data must be unique
                        operation.ChargeId,
                        operation.ChargeOwner,
                        operation.Type,
                        operation.ChargeName,
                        operation.ChargeDescription,
                        operation.StartDateTime,
                        operation.EndDateTime.TimeOrEndDefault(),
                        operation.VatClassification,
                        operation.TaxIndicator,
                        operation.TransparentInvoicing,
                        operation.Resolution,
                        points));
                }
            }

            return result;
        }

        private bool ShouldMakeDataAvailableForActiveGridProviders(ChargeCommandAcceptedEvent acceptedEvent)
        {
            // We only need to notify grid providers if the charge includes tax which are the
            // only charges they do not maintain themselves
            return acceptedEvent.Command.ChargeOperation.TaxIndicator;
        }
    }
}
