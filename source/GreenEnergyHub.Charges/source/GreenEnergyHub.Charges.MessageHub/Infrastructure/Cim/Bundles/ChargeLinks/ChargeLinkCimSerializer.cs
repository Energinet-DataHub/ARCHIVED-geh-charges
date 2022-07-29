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
using System.Xml.Linq;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.AvailableData.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.ChargeLinks
{
    public class ChargeLinkCimSerializer : CimSerializer<AvailableChargeLinksData>
    {
        public ChargeLinkCimSerializer(
            IClock clock,
            ICimIdProvider cimIdProvider)
            : base(clock, cimIdProvider)
        {
        }

        protected override XNamespace GetNamespace(IEnumerable<AvailableChargeLinksData> records)
        {
            return CimChargeLinkConstants.NotifyNamespace;
        }

        protected override XNamespace GetSchemaLocation(IEnumerable<AvailableChargeLinksData> records)
        {
            return CimChargeLinkConstants.NotifySchemaLocation;
        }

        protected override string GetRootElementName(IEnumerable<AvailableChargeLinksData> records)
        {
            return CimChargeLinkConstants.NotifyRootElement;
        }

        protected override DocumentType GetDocumentType(IEnumerable<AvailableChargeLinksData> records)
        {
            return DocumentType.NotifyBillingMasterData;
        }

        protected override XElement GetActivityRecord(XNamespace cimNamespace, AvailableChargeLinksData chargeLink)
        {
            return new XElement(
                cimNamespace + CimMarketDocumentConstants.MarketActivityRecord,
                new XElement(cimNamespace + CimChargeLinkConstants.MarketActivityRecordId, CimIdProvider.GetUniqueId()),
                new XElement(
                    cimNamespace + CimChargeLinkConstants.MeteringPointId,
                    new XAttribute(CimMarketDocumentConstants.CodingScheme, CodingSchemeMapper.Map(CodingScheme.GS1)),
                    chargeLink.MeteringPointId),
                GetChargeGroupElement(cimNamespace, chargeLink));
        }

        private static XElement GetChargeGroupElement(XNamespace cimNamespace, AvailableChargeLinksData chargeLink)
        {
            return new XElement(
                cimNamespace + CimChargeLinkConstants.ChargeGroup,
                GetChargeTypeElement(cimNamespace, chargeLink));
        }

        private static XElement GetChargeTypeElement(XNamespace cimNamespace, AvailableChargeLinksData chargeLink)
        {
            return new XElement(
                cimNamespace + CimChargeLinkConstants.ChargeTypeElement,
                new XElement(
                    cimNamespace + CimChargeLinkConstants.Owner,
                    new XAttribute(CimMarketDocumentConstants.CodingScheme, CodingSchemeMapper.Map(CodingScheme.GS1)),
                    chargeLink.ChargeOwner),
                new XElement(cimNamespace + CimChargeLinkConstants.ChargeType, ChargeTypeMapper.Map(chargeLink.ChargeType)),
                new XElement(cimNamespace + CimChargeLinkConstants.ChargeId, chargeLink.ChargeId),
                new XElement(cimNamespace + CimChargeLinkConstants.Factor, chargeLink.Factor),
                new XElement(cimNamespace + CimChargeLinkConstants.EffectiveDate, chargeLink.StartDateTime.ToString()),
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    chargeLink.EndDateTime.IsEndDefault(),
                    CimChargeLinkConstants.TerminationDate,
                    () => chargeLink.EndDateTime.ToString()));
        }
    }
}
