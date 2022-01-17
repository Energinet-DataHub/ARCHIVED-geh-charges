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
using Energinet.DataHub.Core.Messaging.Protobuf;
using Energinet.DataHub.Core.Messaging.Transport;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandRejected;
using ChargeLinksCommand = GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.ChargeLinksCommand;

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.Internal.ChargeLinksCommandRejected
{
    public class ChargeLinksCommandRejectedInboundMapper
        : ProtobufInboundMapper<GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandRejected.ChargeLinksCommandRejected>
    {
        protected override IInboundMessage Convert(
            GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandRejected.ChargeLinksCommandRejected chargeLinksCommandRejected)
        {
            return new ChargeLinksRejectedEvent(
                chargeLinksCommandRejected.PublishedTime.ToInstant(),
                new ChargeLinksCommand(
                    chargeLinksCommandRejected.ChargeLinksCommand.MeteringPointId,
                    ConvertDocument(chargeLinksCommandRejected.ChargeLinksCommand.Document),
                    chargeLinksCommandRejected.ChargeLinksCommand.ChargeLinks.Select(ConvertChargeLink)
                        .ToList()),
                ConvertValidationError(chargeLinksCommandRejected.ValidationErrors));
        }

        private IEnumerable<ValidationError> ConvertValidationError(
            IEnumerable<ValidationErrorContract> validationErrorContracts)
        {
            return validationErrorContracts.Select(x =>
                new ValidationError((ValidationRuleIdentifier)x.ValidationRuleIdentifier, x.TriggeredBy));
        }

        private static DocumentDto ConvertDocument(Document document)
        {
            return new DocumentDto
            {
                Id = document.Id,
                RequestDate = document.RequestDate.ToInstant(),
                Type = (Domain.MarketParticipants.DocumentType)document.Type,
                CreatedDateTime = document.CreatedDateTime.ToInstant(),
                Sender = MapMarketParticipant(document.Sender),
                Recipient = MapMarketParticipant(document.Recipient),
                IndustryClassification = (Domain.MarketParticipants.IndustryClassification)document.IndustryClassification,
                BusinessReasonCode = (Domain.MarketParticipants.BusinessReasonCode)document.BusinessReasonCode,
            };
        }

        private static MarketParticipantDto MapMarketParticipant(
            Infrastructure.Internal.ChargeLinksCommandRejected.MarketParticipant marketParticipant)
        {
            return new MarketParticipantDto
            {
                Id = marketParticipant.Id,
                BusinessProcessRole = (Domain.MarketParticipants.MarketParticipantRole)marketParticipant.BusinessProcessRole,
            };
        }

        private static ChargeLinkDto ConvertChargeLink(Infrastructure.Internal.ChargeLinksCommandRejected.ChargeLink link)
        {
            return new ChargeLinkDto
            {
                OperationId = link.OperationId,
                StartDateTime = link.StartDateTime.ToInstant(),
                EndDateTime = link.EndDateTime.ToInstant(),
                SenderProvidedChargeId = link.SenderProvidedChargeId,
                Factor = link.Factor,
                ChargeOwnerId = link.ChargeOwnerId,
                ChargeType = (GreenEnergyHub.Charges.Domain.Charges.ChargeType)link.ChargeType,
            };
        }
    }
}
