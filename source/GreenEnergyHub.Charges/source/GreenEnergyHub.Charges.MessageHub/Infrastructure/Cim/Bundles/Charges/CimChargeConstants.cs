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

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.Charges
{
    /// <summary>
    /// Strings used in CIM/XML for elements, namespaces or attributes that we need to
    /// use when parsing and generating a XML document
    ///
    /// This class is responsible for string that are used in the document area of the XML
    /// which is specific to charge link messages
    /// </summary>
    internal static class CimChargeConstants
    {
        internal const string NotifyNamespace = "urn:ediel.org:structure:notifypricelist:0:1";

        internal const string NotifySchemaLocation =
            "urn:ediel.org:structure:notifypricelist:0:1 urn-ediel-org-structure-notifypricelist-0-1.xsd";

        internal const string NotifyRootElement = "NotifyPriceList_MarketDocument";

        internal const string MarketActivityRecordId = "mRID";

        internal const string SnapshotDateTime = "snapshot_DateAndOrTime.dateTime";

        internal const string ChargeGroup = "ChargeGroup";

        internal const string ChargeTypeElement = "ChargeType";

        internal const string ChargeOwner = "chargeTypeOwner_MarketParticipant.mRID";

        internal const string ChargeType = "type";

        internal const string ChargeId = "mRID";

        internal const string ChargeName = "name";

        internal const string ChargeDescription = "description";

        internal const string ChargeResolution = "priceTimeFrame_Period.resolution";

        internal const string StartDateTime = "effectiveDate";

        internal const string EndDateTime = "terminationDate";

        internal const string VatClassification = "VATPayer";

        internal const string TransparentInvoicing = "transparentInvoicing";

        internal const string TaxIndicator = "taxIndicator";

        internal const string SeriesPeriod = "Series_Period";

        internal const string PeriodResolution = "resolution";

        internal const string TimeInterval = "timeInterval";

        internal const string TimeIntervalStart = "start";

        internal const string TimeIntervalEnd = "end";

        internal const string Point = "Point";

        internal const string Position = "position";

        internal const string Price = "price.amount";
    }
}
