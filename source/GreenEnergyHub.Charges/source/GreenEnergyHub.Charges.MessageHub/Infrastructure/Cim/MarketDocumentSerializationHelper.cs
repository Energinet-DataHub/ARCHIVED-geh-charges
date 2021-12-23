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
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.Configuration;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim
{
    public static class MarketDocumentSerializationHelper
    {
        public static IEnumerable<XElement> Serialize(
            XNamespace cimNamespace,
            ICimIdProvider cimIdProvider,
            DocumentType documentType,
            BusinessReasonCode businessReasonCode,
            IHubSenderConfiguration hubSenderConfiguration,
            string recipientId,
            MarketParticipantRole recipientRole,
            IClock clock)
        {
            return new List<XElement>()
            {
                new XElement(cimNamespace + CimMarketDocumentConstants.Id, cimIdProvider.GetUniqueId()),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.Type,
                    DocumentTypeMapper.Map(documentType)),
                new XElement(
                    cimNamespace +
                    CimMarketDocumentConstants.BusinessReasonCode,
                    BusinessReasonCodeMapper.Map(businessReasonCode)),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.IndustryClassification,
                    IndustryClassificationMapper.Map(IndustryClassification.Electricity)),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.SenderId,
                    new XAttribute(
                        CimMarketDocumentConstants.CodingScheme,
                        CodingSchemeMapper.Map(CodingScheme.GS1)),
                    hubSenderConfiguration.GetSenderMarketParticipant().MarketParticipantId),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.SenderBusinessProcessRole,
                    MarketParticipantRoleMapper.Map(
                        hubSenderConfiguration.GetSenderMarketParticipant().BusinessProcessRole)),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.RecipientId,
                    new XAttribute(
                        CimMarketDocumentConstants.CodingScheme,
                        CodingSchemeMapper.Map(CodingScheme.GS1)),
                    recipientId),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.RecipientBusinessProcessRole,
                    MarketParticipantRoleMapper.Map(recipientRole)),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.CreatedDateTime,
                    clock.GetCurrentInstant().ToString()),
            };
        }
    }
}
