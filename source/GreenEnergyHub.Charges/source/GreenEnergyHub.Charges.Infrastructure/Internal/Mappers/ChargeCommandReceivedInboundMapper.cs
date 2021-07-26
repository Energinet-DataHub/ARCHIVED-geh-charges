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
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;
using GreenEnergyHub.Messaging.MessageTypes.Common;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using NodaTime;
using MarketParticipant = GreenEnergyHub.Charges.Domain.MarketDocument.MarketParticipant;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class ChargeCommandReceivedInboundMapper : ProtobufInboundMapper<ChargeCommandReceivedContract>
    {
        protected override IInboundMessage Convert(ChargeCommandReceivedContract obj)
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

        private Document GetDocument(DocumentContract document)
        {
            return new Document
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

        private ChargeOperation GetChargeOperation(ChargeOperationContract chargeOperation)
        {
            return new ChargeOperation();
        }
    }
}
