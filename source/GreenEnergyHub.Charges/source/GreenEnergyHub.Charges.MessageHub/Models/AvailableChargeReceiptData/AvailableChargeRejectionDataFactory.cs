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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData
{
    public class AvailableChargeRejectionDataFactory :
        IAvailableDataFactory<AvailableChargeReceiptData, ChargeCommandRejectedEvent>
    {
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly AvailableChargeReceiptValidationErrorFactory _availableChargeReceiptValidationErrorFactory;

        public AvailableChargeRejectionDataFactory(
            IMessageMetaDataContext messageMetaDataContext,
            AvailableChargeReceiptValidationErrorFactory availableChargeReceiptValidationErrorFactory)
        {
            _messageMetaDataContext = messageMetaDataContext;
            _availableChargeReceiptValidationErrorFactory = availableChargeReceiptValidationErrorFactory;
        }

        public Task<IReadOnlyList<AvailableChargeReceiptData>> CreateAsync(ChargeCommandRejectedEvent input)
        {
            IReadOnlyList<AvailableChargeReceiptData> result = new List<AvailableChargeReceiptData>()
            {
                new AvailableChargeReceiptData(
                    input.Command.Document.Sender.Id, // The original sender is the recipient of the receipt
                    input.Command.Document.Sender.BusinessProcessRole,
                    input.Command.Document.BusinessReasonCode,
                    _messageMetaDataContext.RequestDataTime,
                    Guid.NewGuid(), // ID of each available piece of data must be unique
                    ReceiptStatus.Rejected,
                    input.Command.ChargeOperation.Id,
                    GetReasons(input)),
            };

            return Task.FromResult(result);
        }

        private List<AvailableChargeReceiptValidationError> GetReasons(ChargeCommandRejectedEvent input)
        {
            return input
                .FailedValidationRuleIdentifiers
                .Select(
                    ruleIdentifier => _availableChargeReceiptValidationErrorFactory
                        .Create(ruleIdentifier, input.Command))
                .ToList();
        }
    }
}
