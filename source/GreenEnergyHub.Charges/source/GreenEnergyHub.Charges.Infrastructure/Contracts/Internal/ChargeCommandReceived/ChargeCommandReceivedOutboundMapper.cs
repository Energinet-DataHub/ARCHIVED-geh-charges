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
using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Core.Messaging.Protobuf;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.Internal.ChargeCommandReceived
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
                },
            };

            ConvertPoints(chargeCommandReceivedContract, chargeCommandReceivedEvent.Command.ChargeOperation.Points);

            return chargeCommandReceivedContract;
        }

        private static ChargeOperationContract ConvertChargeOperation(ChargeOperationDto charge)
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

        private static DocumentContract ConvertDocument(DocumentDto document)
        {
            return new DocumentContract
            {
                Id = document.Id,
                RequestDate = document.RequestDate.ToTimestamp().TruncateToSeconds(),
                Type = (DocumentTypeContract)document.Type,
                CreatedDateTime = document.CreatedDateTime.ToTimestamp().TruncateToSeconds(),
                Sender = new MarketParticipantContract
                {
                    Id = document.Sender.Id,
                    BusinessProcessRole = (MarketParticipantRoleContract)document.Sender.BusinessProcessRole,
                },
                Recipient = new MarketParticipantContract
                {
                    Id = document.Recipient.Id,
                    BusinessProcessRole = (MarketParticipantRoleContract)document.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (IndustryClassificationContract)document.IndustryClassification,
                BusinessReasonCode = (BusinessReasonCodeContract)document.BusinessReasonCode,
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
