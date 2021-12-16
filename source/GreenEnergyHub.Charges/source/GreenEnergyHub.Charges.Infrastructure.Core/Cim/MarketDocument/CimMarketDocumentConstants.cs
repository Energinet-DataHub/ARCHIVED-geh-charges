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

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument
{
    /// <summary>
    /// Strings used in CIM/XML for elements, namespaces or attributes that we need to
    /// use when parsing a XML document
    ///
    /// This class is responsible for string that are used in the document area of the XML
    /// which is shared between multiple CIM/XML document types
    /// </summary>
    public static class CimMarketDocumentConstants
    {
        public const string SchemaValidationNamespace = "http://www.w3.org/2001/XMLSchema-instance";

        public const string SchemaNamespaceAbbreviation = "xsi";

        public const string CimNamespaceAbbreviation = "cim";

        public const string Id = "mRID";

        public const string Type = "type";

        public const string BusinessReasonCode = "process.processType";

        public const string IndustryClassification = "businessSector.type";

        public const string SenderId = "sender_MarketParticipant.mRID";

        public const string SenderBusinessProcessRole = "sender_MarketParticipant.marketRole.type";

        public const string RecipientId = "receiver_MarketParticipant.mRID";

        public const string RecipientBusinessProcessRole = "receiver_MarketParticipant.marketRole.type";

        public const string CreatedDateTime = "createdDateTime";

        public const string SchemaLocation = "schemaLocation";

        public const string CodingScheme = "codingScheme";

        public const string MarketActivityRecord = "MktActivityRecord";
    }
}
