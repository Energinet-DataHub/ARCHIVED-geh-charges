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

using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Messaging.Protobuf;
using Document = GreenEnergyHub.Charges.Domain.MarketParticipants.Document;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class LinkCommandReceivedOutboundMapper : ProtobufOutboundMapper<ChargeLinkCommandReceivedEvent>
    {
        protected override Google.Protobuf.IMessage Convert([NotNull]ChargeLinkCommandReceivedEvent chargeLinkCommandReceivedEvent)
        {
            var chargeLinkCommandReceived = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLinkCommandReceived
            {
                PublishedTime = chargeLinkCommandReceivedEvent.PublishedTime.ToTimestamp().TruncateToSeconds(),
                CorrelationId = chargeLinkCommandReceivedEvent.CorrelationId,
            };

            foreach (var chargeLinkCommand in chargeLinkCommandReceivedEvent.ChargeLinkCommands)
            {
                chargeLinkCommandReceived.ChargeLinkCommands.Add(new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLinkCommand
                {
                    Document = ConvertDocument(chargeLinkCommand.Document),
                    ChargeLink = ConvertChargeLink(chargeLinkCommand.ChargeLink),
                    CorrelationId = chargeLinkCommandReceivedEvent.CorrelationId,
                });
            }

            return chargeLinkCommandReceived;
        }

        private static GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.Document ConvertDocument(Document document)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.Document
            {
                Id = document.Id,
                RequestDate = document.RequestDate.ToTimestamp(),
                Type = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.DocumentType)document.Type,
                CreatedDateTime = document.CreatedDateTime.ToTimestamp(),
                Sender = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.MarketParticipant
                {
                    Id = document.Sender.Id,
                    BusinessProcessRole = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.MarketParticipantRole)document.Sender.BusinessProcessRole,
                },
                Recipient = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.MarketParticipant
                {
                    Id = document.Recipient.Id,
                    BusinessProcessRole = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.MarketParticipantRole)document.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.IndustryClassification)document.IndustryClassification,
                BusinessReasonCode = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.BusinessReasonCode)document.BusinessReasonCode,
            };
        }

        private static GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLink ConvertChargeLink(ChargeLinkDto chargeLinkDto)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLink
            {
                OperationId = chargeLinkDto.OperationId,
                MeteringPointId = chargeLinkDto.MeteringPointId,
                SenderProvidedChargeId = chargeLinkDto.SenderProvidedChargeId,
                ChargeOwner = chargeLinkDto.ChargeOwner,
                Factor = chargeLinkDto.Factor,
                ChargeType = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeType)chargeLinkDto.ChargeType,
                StartDateTime = chargeLinkDto.StartDateTime.ToTimestamp(),
                EndDateTime = chargeLinkDto.EndDateTime?.ToTimestamp(),
            };
        }
    }
}
