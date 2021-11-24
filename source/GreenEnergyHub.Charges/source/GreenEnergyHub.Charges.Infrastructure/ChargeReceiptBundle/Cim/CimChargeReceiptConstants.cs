﻿// Copyright 2020 Energinet DataHub A/S
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

namespace GreenEnergyHub.Charges.Infrastructure.ChargeReceiptBundle.Cim
{
    /// <summary>
    /// Strings used in CIM/XML for elements, namespaces or attributes that we need to
    /// use when parsing and generating a XML document
    ///
    /// This class is responsible for strings that are used in the document area of the XML
    /// which is specific to charge link messages
    /// </summary>
    internal static class CimChargeReceiptConstants
    {
        internal const string ConfirmNamespace = "urn:ediel.org:structure:confirmrequestchangeofpricelist:0:1";

        internal const string ConfirmSchemaLocation =
            "urn:ediel.org:structure:confirmrequestchangeofpricelist:0:1 urn-ediel-org-structure-confirmrequestchangeofpricelist-0-1.xsd";

        internal const string ConfirmRootElement = "ConfirmRequestChangeOfPriceList_MarketDocument";

        internal const string RejectNamespace = "urn:ediel.org:structure:rejectrequestchangeofpricelist:0:1";

        internal const string RejectSchemaLocation =
            "urn:ediel.org:structure:rejectrequestchangeofpricelist:0:1 urn-ediel-org-structure-rejectrequestchangeofpricelist-0-1.xsd";

        internal const string RejectRootElement = "RejectRequestChangeOfPriceList_MarketDocument";

        internal const string ReceiptStatus = "reason.code"; // Note: There are two reason codes in the CIM document

        internal const string Id = "mRID";

        internal const string OriginalOperationId = "originalTransactionIDReference_MktActivityRecord.mRID";

        internal const string ReasonElement = "Reason";

        internal const string ReasonCode = "code"; // Note: There are two reason codes in the CIM document

        internal const string ReasonText = "text";
    }
}
