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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData
{
    public class AvailableChargeLinksRejectionDataFactory : IAvailableDataFactory<AvailableChargeLinksReceiptData, ChargeLinksRejectedEvent>
    {
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly IAvailableChargeLinksReceiptValidationErrorFactory _availableChargeLinksReceiptValidationErrorFactory;

        public AvailableChargeLinksRejectionDataFactory(
            IMessageMetaDataContext messageMetaDataContext,
            IAvailableChargeLinksReceiptValidationErrorFactory availableChargeLinksReceiptValidationErrorFactory)
        {
            _messageMetaDataContext = messageMetaDataContext;
            _availableChargeLinksReceiptValidationErrorFactory = availableChargeLinksReceiptValidationErrorFactory;
        }

        public Task<IReadOnlyList<AvailableChargeLinksReceiptData>> CreateAsync(ChargeLinksRejectedEvent input)
        {
            IReadOnlyList<AvailableChargeLinksReceiptData> result =
                input.ChargeLinksCommand.ChargeLinks.Select(chargeLinkDto => new AvailableChargeLinksReceiptData(
                    input.ChargeLinksCommand.Document.Sender.Id, // The original sender is the recipient of the receipt
                    input.ChargeLinksCommand.Document.Recipient.BusinessProcessRole,
                    input.ChargeLinksCommand.Document.BusinessReasonCode,
                    _messageMetaDataContext.RequestDataTime,
                    Guid.NewGuid(), // ID of each available piece of data must be unique
                    ReceiptStatus.Rejected,
                    chargeLinkDto.OperationId,
                    input.ChargeLinksCommand.MeteringPointId,
                    GetReasons(input))).ToList();

            return Task.FromResult(result);
        }

        private List<AvailableReceiptValidationError> GetReasons(ChargeLinksRejectedEvent input)
        {
            return input
                .FailedValidationRuleIdentifiers
                .Select(
                    ruleIdentifier => _availableChargeLinksReceiptValidationErrorFactory
                        .Create(ruleIdentifier, input.ChargeLinksCommand))
                .ToList();
        }
    }
}
