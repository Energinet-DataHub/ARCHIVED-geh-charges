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
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class ChargeCommandReceivedOutboundMapper : ProtobufOutboundMapper<ChargeCommand>
    {
        protected override Google.Protobuf.IMessage Convert(ChargeCommand obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var document = obj.Document;
            var charge = obj.ChargeOperation;

            var chargeCommandReceivedContract = new ChargeCommandReceivedContract
            {
                Document = new DocumentContract
                {
                    Id = document.Id,
                    RequestDate = Timestamp.FromDateTime(document.RequestDate.ToDateTimeUtc()),
                    Type = (DocumentTypeContract)document.Type,
                    CreatedDateTime = Timestamp.FromDateTime(document.CreatedDateTime.ToDateTimeUtc()),
                    Sender = new MarketParticipantContract
                    {
                        Id = document.Sender.Id,
                        MarketParticipantRole = (MarketParticipantRoleContract)document.Sender.BusinessProcessRole,
                    },
                    Recipient = new MarketParticipantContract
                    {
                        Id = document.Recipient.Id,
                        MarketParticipantRole = (MarketParticipantRoleContract)document.Recipient.BusinessProcessRole,
                    },
                    IndustryClassification = (IndustryClassificationContract)document.IndustryClassification,
                    BusinessReasonCode = (BusinessReasonCodeContract)document.BusinessReasonCode,
                },
                ChargeOperation = new ChargeOperationContract
                {
                    Id = obj.ChargeOperation.Id,
                    ChargeId = charge.ChargeId,
                    ChargeOwner = charge.ChargeOwner,
                    ChargeType = (ChargeTypeContract)charge.Type,
                    StartDateTime = Timestamp.FromDateTime(charge.StartDateTime.ToDateTimeUtc()),
                    EndDateTime = Timestamp.FromDateTime(charge.EndDateTime.TimeOrEndDefault().ToDateTimeUtc()),
                    Resolution = (ResolutionContract)charge.Resolution,
                    ChargeDescription = charge.ChargeDescription,
                    ChargeName = charge.ChargeName,
                    OperationType = (OperationTypeContract)charge.OperationType,
                    TaxIndicator = charge.TaxIndicator,
                    TransparentInvoicing = charge.TransparentInvoicing,
                    VatClassification = (VatClassificationContract)charge.VatClassification,
                },
                CorrelationId = obj.CorrelationId,
            };

            AddChargePoints(chargeCommandReceivedContract, charge.Points);

            return chargeCommandReceivedContract;
        }

        private void AddChargePoints(ChargeCommandReceivedContract contract, List<Point> points)
        {
            foreach (Point point in points)
            {
                contract.ChargeOperation.Points.Add(new PointContract
                {
                    Position = point.Position,
                    Price = (double)point.Price,
                    Time = Timestamp.FromDateTime(point.Time.ToDateTimeUtc()),
                });
            }
        }
    }
}
