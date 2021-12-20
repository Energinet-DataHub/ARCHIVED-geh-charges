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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.Internal.ChargeCommandRejected
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
                },
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

        private static ChargeOperationContract ConvertChargeOperation(ChargeOperationDto charge)
        {
            return new ChargeOperationContract
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
