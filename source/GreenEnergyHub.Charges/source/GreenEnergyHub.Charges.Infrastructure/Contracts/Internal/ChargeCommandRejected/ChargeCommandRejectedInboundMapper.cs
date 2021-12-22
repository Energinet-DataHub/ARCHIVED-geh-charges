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
using Energinet.DataHub.Core.Messaging.MessageTypes.Common;
using Energinet.DataHub.Core.Messaging.Protobuf;
using Energinet.DataHub.Core.Messaging.Transport;
using Google.Protobuf.Collections;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.Internal.ChargeCommandRejected
{
    public class ChargeCommandRejectedInboundMapper : ProtobufInboundMapper<ChargeCommandRejectedContract>
    {
        protected override IInboundMessage Convert(ChargeCommandRejectedContract rejectedContract)
        {
            return new ChargeCommandRejectedEvent(
                rejectedContract.PublishedTime.ToInstant(),
                new ChargeCommand
                {
                    Document = ConvertDocument(rejectedContract.Command.Document),
                    ChargeOperation = ConvertChargeOperation(rejectedContract.Command.ChargeOperation),
                    Transaction = Transaction.NewTransaction(),
                },
                ConvertValidationErrors(rejectedContract.ValidationErrors));
        }

        private IEnumerable<ValidationError> ConvertValidationErrors(
            RepeatedField<ValidationErrorContract> validationErrors)
        {
            return validationErrors.Select(x => new ValidationError(
                    (ValidationRuleIdentifier)x.ValidationRuleIdentifier,
                    x.ValidationErrorMessageParameters.Select(y =>
                        new ValidationErrorMessageParameter(
                            y.MessageParameter,
                            (ValidationErrorMessageParameterType)y.ParameterType))
                        .ToArray()));
        }

        private static DocumentDto ConvertDocument(DocumentContract document)
        {
            return new DocumentDto
            {
                Id = document.Id,
                Sender =
                    new MarketParticipantDto
                    {
                        Id = document.Sender.Id,
                        BusinessProcessRole = (MarketParticipantRole)document.Sender.BusinessProcessRole,
                    },
                Recipient =
                    new MarketParticipantDto
                    {
                        Id = document.Recipient.Id,
                        BusinessProcessRole = (MarketParticipantRole)document.Recipient.BusinessProcessRole,
                    },
                Type = (DocumentType)document.Type,
                IndustryClassification = (IndustryClassification)document.IndustryClassification,
                RequestDate = Instant.FromUnixTimeSeconds(document.RequestDate.Seconds),
                BusinessReasonCode = (BusinessReasonCode)document.BusinessReasonCode,
                CreatedDateTime = document.CreatedDateTime.ToInstant(),
            };
        }

        private static ChargeOperationDto ConvertChargeOperation(ChargeOperationContract chargeOperation)
        {
            return new ChargeOperationDto(
                chargeOperation.Id,
                (ChargeType)chargeOperation.Type,
                chargeOperation.ChargeId,
                chargeOperation.ChargeName,
                chargeOperation.ChargeDescription,
                chargeOperation.ChargeOwner,
                (Resolution)chargeOperation.Resolution,
                chargeOperation.TaxIndicator,
                chargeOperation.TransparentInvoicing,
                (VatClassification)chargeOperation.VatClassification,
                chargeOperation.StartDateTime.ToInstant(),
                chargeOperation.EndDateTime.ToInstant(),
                ConvertPoints(chargeOperation.Points));
        }

        private static List<Point> ConvertPoints(RepeatedField<PointContract> points)
        {
            return points
                .Select(point => new Point(point.Position, (decimal)point.Price, point.Time.ToInstant()))
                .ToList();
        }
    }
}
