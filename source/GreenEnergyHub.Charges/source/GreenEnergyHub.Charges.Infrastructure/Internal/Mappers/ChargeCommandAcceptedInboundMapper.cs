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

using System;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandAccepted;
using GreenEnergyHub.Messaging.MessageTypes.Common;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using NodaTime;
using MarketParticipant = GreenEnergyHub.Charges.Domain.MarketDocument.MarketParticipant;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class ChargeCommandAcceptedInboundMapper : ProtobufInboundMapper<ChargeCommandAcceptedContract>
    {
        protected override IInboundMessage Convert(ChargeCommandAcceptedContract obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return new ChargeCommandReceivedEvent(
                Instant.FromDateTimeUtc(DateTime.Now.ToUniversalTime()),
                obj.CorrelationId,
                new ChargeCommand(obj.CorrelationId)
            {
                Document = GetDocument(obj.Document),
                ChargeOperation = GetChargeOperation(obj.ChargeOperation),
                Transaction = Transaction.NewTransaction(),
            });
        }

        private static Document GetDocument(DocumentContract document)
        {
            return new ()
            {
                Id = document.Id,
                Sender =
                    new MarketParticipant
                    {
                        Id = document.Sender.Id,
                        BusinessProcessRole = (MarketParticipantRole)document.Sender.MarketParticipantRole,
                    },
                Recipient =
                    new MarketParticipant
                    {
                        Id = document.Recipient.Id,
                        BusinessProcessRole = (MarketParticipantRole)document.Recipient.MarketParticipantRole,
                    },
                Type = (DocumentType)document.Type,
                IndustryClassification = (IndustryClassification)document.IndustryClassification,
                RequestDate = Instant.FromUnixTimeSeconds(document.RequestDate.Seconds),
                BusinessReasonCode = (BusinessReasonCode)document.BusinessReasonCode,
                CreatedDateTime = Instant.FromUnixTimeSeconds(document.CreatedDateTime.Seconds),
            };
        }

        private static ChargeOperation GetChargeOperation(ChargeOperationContract chargeOperation)
        {
            return new ()
            {
                Id = chargeOperation.Id,
                Resolution = (Resolution)chargeOperation.Resolution,
                Type = (ChargeType)chargeOperation.ChargeType,
                ChargeDescription = chargeOperation.ChargeDescription,
                ChargeId = chargeOperation.ChargeId,
                ChargeName = chargeOperation.ChargeName,
                ChargeOwner = chargeOperation.ChargeOwner,
                OperationType = (OperationType)chargeOperation.OperationType,
                TaxIndicator = chargeOperation.TaxIndicator,
                TransparentInvoicing = chargeOperation.TransparentInvoicing,
                VatClassification = (VatClassification)chargeOperation.VatClassification,
                StartDateTime = Instant.FromUnixTimeSeconds(chargeOperation.StartDateTime.Seconds),
                EndDateTime = Instant.FromUnixTimeSeconds(chargeOperation.EndDateTime.Seconds),
            };
        }
    }
}
