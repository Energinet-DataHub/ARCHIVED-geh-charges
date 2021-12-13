﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Core.Messaging.MessageTypes.Common;
using Energinet.DataHub.Core.Messaging.Protobuf;
using Energinet.DataHub.Core.Messaging.Transport;
using Google.Protobuf.Collections;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;
using NodaTime;
using MarketParticipant = GreenEnergyHub.Charges.Domain.MarketParticipants.MarketParticipant;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class ChargeCommandReceivedInboundMapper : ProtobufInboundMapper<ChargeCommandReceivedContract>
    {
        protected override IInboundMessage Convert(ChargeCommandReceivedContract chargeCommandReceivedContract)
        {
            return new ChargeCommandReceivedEvent(
                chargeCommandReceivedContract.PublishedTime.ToInstant(),
                new ChargeCommand
            {
                Document = ConvertDocument(chargeCommandReceivedContract.Command.Document),
                ChargeOperation = ConvertChargeOperation(chargeCommandReceivedContract.Command.ChargeOperation),
                Transaction = Transaction.NewTransaction(),
            });
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

        private static ChargeOperation ConvertChargeOperation(ChargeOperationContract chargeOperation)
        {
            return new ChargeOperation(
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
            return new ChargeOperationDto
            {
                Id = chargeOperation.Id,
                Resolution = (Resolution)chargeOperation.Resolution,
                Type = (ChargeType)chargeOperation.Type,
                ChargeDescription = chargeOperation.ChargeDescription,
                ChargeId = chargeOperation.ChargeId,
                ChargeName = chargeOperation.ChargeName,
                ChargeOwner = chargeOperation.ChargeOwner,
                TaxIndicator = chargeOperation.TaxIndicator,
                TransparentInvoicing = chargeOperation.TransparentInvoicing,
                VatClassification = (VatClassification)chargeOperation.VatClassification,
                StartDateTime = chargeOperation.StartDateTime.ToInstant(),
                EndDateTime = chargeOperation.EndDateTime?.ToInstant(),
                Points = ConvertPoints(chargeOperation.Points),
            };
        }

        private static List<Point> ConvertPoints(RepeatedField<PointContract> points)
        {
            var list = new List<Point>();

            foreach (var point in points)
            {
                list.Add(new Point(point.Position, (decimal)point.Price, point.Time.ToInstant()));
            }

            return list;
        }
    }
}
