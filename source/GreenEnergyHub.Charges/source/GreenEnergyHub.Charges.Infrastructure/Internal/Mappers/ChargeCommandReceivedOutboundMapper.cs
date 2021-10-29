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
using System.Diagnostics.CodeAnalysis;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class ChargeCommandReceivedOutboundMapper : ProtobufOutboundMapper<ChargeCommandReceivedEvent>
    {
        protected override Google.Protobuf.IMessage Convert([NotNull]ChargeCommandReceivedEvent chargeCommandReceivedEvent)
        {
            var chargeCommandReceivedContract = new ChargeCommandReceivedContract
            {
                PublishedTime = chargeCommandReceivedEvent.PublishedTime.ToTimestamp().TruncateToSeconds(),
                Command = new ChargeCommandContract
                {
                    Document = ConvertDocument(chargeCommandReceivedEvent.Command.Document),
                    ChargeOperation = ConvertChargeOperation(chargeCommandReceivedEvent.Command.ChargeOperation),
                    CorrelationId = chargeCommandReceivedEvent.Command.CorrelationId,
                },
                CorrelationId = chargeCommandReceivedEvent.CorrelationId,
            };

            ConvertPoints(chargeCommandReceivedContract, chargeCommandReceivedEvent.Command.ChargeOperation.Points);

            return chargeCommandReceivedContract;
        }

        private static ChargeOperationContract ConvertChargeOperation(ChargeOperation charge)
        {
            return new ChargeOperationContract
            {
                Id = charge.Id,
                ChargeId = charge.ChargeId,
                ChargeOwner = charge.ChargeOwner,
                Type = (ChargeTypeContract)charge.Type,
                StartDateTime = charge.StartDateTime.ToTimestamp().TruncateToSeconds(),
                EndDateTime = charge.EndDateTime?.ToTimestamp().TruncateToSeconds(),
                Resolution = (ResolutionContract)charge.Resolution,
                ChargeDescription = charge.ChargeDescription,
                ChargeName = charge.ChargeName,
                TaxIndicator = charge.TaxIndicator,
                TransparentInvoicing = charge.TransparentInvoicing,
                VatClassification = (VatClassificationContract)charge.VatClassification,
            };
        }

        private static DocumentContract ConvertDocument(DocumentDto documentDto)
        {
            return new DocumentContract
            {
                Id = documentDto.Id,
                RequestDate = documentDto.RequestDate.ToTimestamp().TruncateToSeconds(),
                Type = (DocumentTypeContract)documentDto.Type,
                CreatedDateTime = documentDto.CreatedDateTime.ToTimestamp().TruncateToSeconds(),
                Sender = new MarketParticipantContract
                {
                    Id = documentDto.Sender.Id,
                    BusinessProcessRole = (MarketParticipantRoleContract)documentDto.Sender.BusinessProcessRole,
                },
                Recipient = new MarketParticipantContract
                {
                    Id = documentDto.Recipient.Id,
                    BusinessProcessRole = (MarketParticipantRoleContract)documentDto.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (IndustryClassificationContract)documentDto.IndustryClassification,
                BusinessReasonCode = (BusinessReasonCodeContract)documentDto.BusinessReasonCode,
            };
        }

        private static void ConvertPoints(ChargeCommandReceivedContract contract, List<Point> points)
        {
            foreach (Point point in points)
            {
                contract.Command.ChargeOperation.Points.Add(new PointContract
                {
                    Position = point.Position,
                    Price = (double)point.Price,
                    Time = point.Time.ToTimestamp().TruncateToSeconds(),
                });
            }
        }
    }
}
