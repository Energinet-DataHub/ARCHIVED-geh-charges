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
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class ChargeCommandRejectedOutboundMapper : ProtobufOutboundMapper<ChargeCommandRejectedEvent>
    {
        protected override Google.Protobuf.IMessage Convert([NotNull]ChargeCommandRejectedEvent rejectionEvent)
        {
            var chargeCommandRejectedContract = new ChargeCommandRejectedContract
            {
                PublishedTime = rejectionEvent.PublishedTime.ToTimestamp().TruncateToSeconds(),
                Command = new ChargeCommandContract
                {
                    Document = ConvertDocument(rejectionEvent.Command.Document),
                    ChargeOperation = ConvertChargeOperation(rejectionEvent.Command.ChargeOperation),
                    CorrelationId = rejectionEvent.Command.CorrelationId,
                },
                CorrelationId = rejectionEvent.CorrelationId,
            };

            ConvertPoints(chargeCommandRejectedContract, rejectionEvent.Command.ChargeOperation.Points);
            AddRejectedReasons(chargeCommandRejectedContract, rejectionEvent);

            return chargeCommandRejectedContract;
        }

        private static void AddRejectedReasons(ChargeCommandRejectedContract chargeCommandRejectedContract, ChargeCommandRejectedEvent rejectionEvent)
        {
            foreach (string reason in rejectionEvent.RejectReasons)
            {
                chargeCommandRejectedContract.RejectReasons.Add(reason);
            }
        }

        private static ChargeOperationContract ConvertChargeOperation(ChargeOperation charge)
        {
            return new ()
            {
                Id = charge.Id,
                ChargeId = charge.ChargeId,
                ChargeOwner = charge.ChargeOwner,
                Type = (ChargeTypeContract)charge.Type,
                StartDateTime = charge.StartDateTime.ToTimestamp().TruncateToSeconds(),
                EndDateTime = charge.EndDateTime.TimeOrEndDefault().ToTimestamp().TruncateToSeconds(),
                Resolution = (ResolutionContract)charge.Resolution,
                ChargeDescription = charge.ChargeDescription,
                ChargeName = charge.ChargeName,
                TaxIndicator = charge.TaxIndicator,
                TransparentInvoicing = charge.TransparentInvoicing,
                VatClassification = (VatClassificationContract)charge.VatClassification,
            };
        }

        private static DocumentContract ConvertDocument(Document document)
        {
            return new ()
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

        private static void ConvertPoints(ChargeCommandRejectedContract contract, List<Point> points)
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
