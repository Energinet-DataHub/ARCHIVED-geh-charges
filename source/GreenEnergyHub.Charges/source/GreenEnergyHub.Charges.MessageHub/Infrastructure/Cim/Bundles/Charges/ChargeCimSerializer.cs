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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.Charges
{
    public class ChargeCimSerializer : CimSerializer<AvailableChargeData>
    {
        public ChargeCimSerializer(IClock clock, ICimIdProvider cimIdProvider)
            : base(clock, cimIdProvider)
        {
        }

        protected override XNamespace GetNamespace(IEnumerable<AvailableChargeData> records)
        {
            return CimChargeConstants.NotifyNamespace;
        }

        protected override XNamespace GetSchemaLocation(IEnumerable<AvailableChargeData> records)
        {
            return CimChargeConstants.NotifySchemaLocation;
        }

        protected override string GetRootElementName(IEnumerable<AvailableChargeData> records)
        {
            return CimChargeConstants.NotifyRootElement;
        }

        protected override DocumentType GetDocumentType(IEnumerable<AvailableChargeData> records)
        {
            return DocumentType.NotifyPriceList;
        }

        protected override XElement GetActivityRecord(
            XNamespace cimNamespace,
            AvailableChargeData record)
        {
            return new XElement(
                cimNamespace + CimMarketDocumentConstants.MarketActivityRecord,
                new XElement(cimNamespace + CimChargeConstants.MarketActivityRecordId, CimIdProvider.GetUniqueId()),
                new XElement(cimNamespace + CimChargeConstants.SnapshotDateTime, record.RequestDateTime.ToString()),
                GetChargeGroupElement(cimNamespace, record));
        }

        private static XElement GetChargeGroupElement(
            XNamespace cimNamespace,
            AvailableChargeData charge)
        {
            return new XElement(
                cimNamespace + CimChargeConstants.ChargeGroup,
                GetChargeInformationTypeElement(cimNamespace, charge));
        }

        private static XElement GetChargeInformationTypeElement(
            XNamespace cimNamespace,
            AvailableChargeData charge)
        {
            return new XElement(
                cimNamespace + CimChargeConstants.ChargeTypeElement,
                new XElement(
                    cimNamespace + CimChargeConstants.ChargeOwner,
                    new XAttribute(
                        CimMarketDocumentConstants.CodingScheme,
                        CodingSchemeMapper.Map(CodingScheme.GS1)),
                    charge.ChargeOwner),
                new XElement(cimNamespace + CimChargeConstants.ChargeType, ChargeTypeMapper.Map(charge.ChargeType)),
                new XElement(cimNamespace + CimChargeConstants.ChargeId, charge.ChargeId),
                // Charge name
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    string.IsNullOrEmpty(charge.ChargeName),
                    CimChargeConstants.ChargeName,
                    () => charge.ChargeName),
                // Charge description
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    string.IsNullOrEmpty(charge.ChargeDescription),
                    CimChargeConstants.ChargeDescription,
                    () => charge.ChargeDescription),
                // Charge resolution
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    charge.Resolution == Resolution.Unknown,
                    CimChargeConstants.ChargeResolution,
                    () => ResolutionMapper.Map(charge.Resolution)),
                // EffectiveDate
                new XElement(cimNamespace + CimChargeConstants.EffectiveDate, charge.StartDateTime.ToString()),
                // TerminationDate
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    charge.EndDateTime.IsEndDefault(),
                    CimChargeConstants.TerminationDate,
                    () => charge.EndDateTime.ToString()),
                // VatClassification
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    charge.VatClassification == VatClassification.Unknown,
                    CimChargeConstants.VatClassification,
                    () => VatClassificationMapper.Map(charge.VatClassification)),
                // TransparentInvoicing
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    // Right now, charge name is our best bet of determining whether to include transparent invoicing
                    string.IsNullOrEmpty(charge.ChargeName),
                    CimChargeConstants.TransparentInvoicing,
                    () => charge.TransparentInvoicing),
                // TaxIndicator
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    // Right now, charge name is our best bet of determining whether to include tax indicator
                    string.IsNullOrEmpty(charge.ChargeName),
                    CimChargeConstants.TaxIndicator,
                    () => charge.TaxIndicator));
        }
    }
}
