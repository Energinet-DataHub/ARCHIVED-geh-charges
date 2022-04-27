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

namespace GreenEnergyHub.Charges.Infrastructure.CimDeserialization.ChargeLinkBundle
{
    /// <summary>
    /// Strings used in CIM/XML for elements, namespaces or attributes that we need to
    /// use when parsing a XML document
    ///
    /// This class is specifically used for string specific to the time series command messages
    /// </summary>
    internal static class CimChargeLinkCommandConstants
    {
        /// <summary>
        /// The CIM namespace for now includes ebix in the naming. This has been raised to the CIM group, but remains as is for now
        /// </summary>
        internal const string Namespace = "urn:ediel.org:structure:requestchangebillingmasterdata:0:1";

        internal const string MktActivityRecord = "MktActivityRecord";

        internal const string Id = "mRID";

        internal const string MeteringPointId = "marketEvaluationPoint.mRID";

        internal const string ChargeGroup = "chargeGroup";

        internal const string ChargeType = "chargeType";

        internal const string Type = "type";

        internal const string ChargeId = "mRID";

        internal const string Factor = "factor";

        internal const string ChargeOwner = "chargeTypeOwner_MarketParticipant.mRID";

        internal const string EffectiveDate = "effectiveDate";

        internal const string TerminationDate = "terminationDate";
    }
}
