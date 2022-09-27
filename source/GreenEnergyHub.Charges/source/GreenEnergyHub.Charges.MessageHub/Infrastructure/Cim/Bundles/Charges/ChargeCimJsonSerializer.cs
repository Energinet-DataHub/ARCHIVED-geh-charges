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
using System.Text.Json.Nodes;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.Charges
{
    public class ChargeCimJsonSerializer : CimJsonSerializer<AvailableChargeData>
    {
        public ChargeCimJsonSerializer(IClock clock, ICimIdProvider cimIdProvider)
            : base(clock, cimIdProvider)
        {
        }

        protected override string GetRootElementName()
        {
            return CimChargeConstants.NotifyRootElement;
        }

        protected override DocumentType GetDocumentType()
        {
            return DocumentType.NotifyPriceList;
        }

        protected override JsonObject GetActivityRecord(AvailableChargeData record)
        {
            ArgumentNullException.ThrowIfNull(record);
            var activity = new JsonObject
            {
                    { CimChargeConstants.MarketActivityRecordId, CimIdProvider.GetUniqueId() },
                    { CimChargeConstants.ChargeGroup, GetChargeGroup(record) },
            };
            return activity;
        }

        private static JsonObject GetChargeGroup(AvailableChargeData charge)
        {
            var chargeGroup = new JsonObject()
            {
                { CimChargeConstants.ChargeTypeElement, GetChargeType(charge) },
            };

            return chargeGroup;
        }

        private static JsonArray GetChargeType(AvailableChargeData charge)
        {
            var chargeType = new JsonArray() { GetChargeInformation(charge) };
            return chargeType;
        }

        private static JsonObject GetChargeInformation(AvailableChargeData charge)
        {
            var chargeInformation = new JsonObject()
            {
                { CimChargeConstants.ChargeId, charge.ChargeId },
                {
                    CimChargeConstants.VatClassification,
                    CimJsonHelper.CreateValueObject(VatClassificationMapper.Map(charge.VatClassification))
                },
                { CimChargeConstants.ChargeOwner, CimJsonHelper.CreateValueObject(charge.ChargeOwner, CodingScheme.GS1) },
                { CimChargeConstants.ChargeDescription, charge.ChargeDescription },
                { CimChargeConstants.EffectiveDate, charge.StartDateTime.ToString() },
                { CimChargeConstants.ChargeName, charge.ChargeName },
                { CimChargeConstants.ChargeResolution, ResolutionMapper.Map(charge.Resolution) },
                { CimChargeConstants.TaxIndicator, charge.TaxIndicator.ToString() },
                { CimChargeConstants.TerminationDate, charge.EndDateTime.ToString() },
                { CimChargeConstants.TransparentInvoicing, charge.TransparentInvoicing.ToString() },
                { CimChargeConstants.ChargeType, CimJsonHelper.CreateValueObject(ChargeTypeMapper.Map(charge.ChargeType)) },
            };
            return chargeInformation;
        }
    }
}
