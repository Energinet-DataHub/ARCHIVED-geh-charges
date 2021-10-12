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

namespace GreenEnergyHub.Charges.Infrastructure.MarketDocument.Cim
{
    /// <summary>
    /// Strings used in CIM/XML for elements, namespaces or attributes that we need to
    /// use when parsing a XML document
    ///
    /// This class is responsible for string that are used in the document area of the XML
    /// which is shared between multiple CIM/XML document types
    /// </summary>
    internal static class CimMarketDocumentConstants
    {
        internal const string SchemaValidationNamespace = "http://www.w3.org/2001/XMLSchema-instance";

        internal const string SchemaNamespaceAbbreviation = "xsi";

        internal const string CimNamespaceAbbreviation = "cim";

        internal const string Id = "mRID";

        internal const string Type = "type";

        internal const string BusinessReasonCode = "process.processType";

        internal const string IndustryClassification = "businessSector.type";

        internal const string SenderId = "sender_MarketParticipant.mRID";

        internal const string SenderBusinessProcessRole = "sender_MarketParticipant.marketRole.type";

        internal const string RecipientId = "receiver_MarketParticipant.mRID";

        internal const string RecipientBusinessProcessRole = "receiver_MarketParticipant.marketRole.type";

        internal const string CreatedDateTime = "createdDateTime";

        internal const string SchemaLocation = "schemaLocation";

        internal const string CodingScheme = "codingScheme";

        internal const string MarketActivityRecord = "MktActivityRecord";
    }
}
