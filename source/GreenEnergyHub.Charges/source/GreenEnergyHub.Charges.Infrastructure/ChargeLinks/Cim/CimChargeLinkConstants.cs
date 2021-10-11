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

namespace GreenEnergyHub.Charges.Infrastructure.ChargeLinks.Cim
{
    /// <summary>
    /// Strings used in CIM/XML for elements, namespaces or attributes that we need to
    /// use when parsing a XML document
    ///
    /// This class is responsible for string that are used in the document area of the XML
    /// which is shared between multiple CIM/XML document types
    /// </summary>
    internal static class CimChargeLinkConstants
    {
        internal const string MarketActivityRecordId = "mRID";

        internal const string MeteringPointId = "marketEvaluationPoint.mRID";

        internal const string StartDateTime = "start_DateAndOrTime.dateTime";

        internal const string EndDateTime = "end_DateAndOrTime.dateTime";
    }
}
