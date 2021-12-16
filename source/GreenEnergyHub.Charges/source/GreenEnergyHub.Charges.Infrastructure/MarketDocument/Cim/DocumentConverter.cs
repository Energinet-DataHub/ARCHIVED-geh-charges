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

using System.Threading.Tasks;
using System.Xml;
using Energinet.DataHub.Core.Messaging.Transport;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.MarketDocument.Cim
{
    public abstract class DocumentConverter
    {
        private readonly IClock _clock;

        protected DocumentConverter(IClock clock)
        {
            _clock = clock;
        }

        public async Task<IInboundMessage> ConvertAsync(XmlReader reader)
        {
            var document = await ParseDocumentAsync(reader).ConfigureAwait(false);

            var message = await ConvertSpecializedContentAsync(reader, document).ConfigureAwait(false);

            return message;
        }

        protected abstract Task<IInboundMessage> ConvertSpecializedContentAsync(XmlReader reader, DocumentDto document);

        private static bool RootElementNotFound(XmlReader reader, string rootElement, string rootNamespace)
        {
            return reader.NodeType != XmlNodeType.Element
                   && rootElement.Length == 0
                   && rootNamespace.Length == 0;
        }

        private static bool IfRootElementIsNotAssigned(string rootElement, string rootNamespace)
        {
            return rootElement.Length == 0 && rootNamespace.Length == 0;
        }

        private static async Task ParseFieldsAsync(XmlReader reader, DocumentDto document)
        {
            string rootElement = string.Empty;
            string ns = string.Empty;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (RootElementNotFound(reader, rootElement, ns))
                {
                    continue;
                }

                if (IfRootElementIsNotAssigned(rootElement, ns))
                {
                    rootElement = reader.LocalName;
                    ns = reader.NamespaceURI;
                }
                else if (reader.Is(CimMarketDocumentConstants.Id, ns))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    document.Id = content;
                }
                else if (reader.Is(CimMarketDocumentConstants.Type, ns))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    document.Type = DocumentTypeMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.BusinessReasonCode, ns))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    document.BusinessReasonCode = BusinessReasonCodeMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.IndustryClassification, ns))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    document.IndustryClassification = IndustryClassificationMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.SenderId, ns))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    document.Sender.Id = content;
                }
                else if (reader.Is(CimMarketDocumentConstants.SenderBusinessProcessRole, ns))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    document.Sender.BusinessProcessRole = MarketParticipantRoleMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.RecipientId, ns))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    document.Recipient.Id = content;
                }
                else if (reader.Is(CimMarketDocumentConstants.RecipientBusinessProcessRole, ns))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    document.Recipient.BusinessProcessRole = MarketParticipantRoleMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.CreatedDateTime, ns))
                {
                    document.CreatedDateTime = Instant.FromDateTimeUtc(reader.ReadElementContentAsDateTime());
                }
                else if (reader.IsElement())
                {
                    // CIM does not have the payload in a separate element,
                    // so we have to assume that the first unknown element
                    // is the start of the specialized document
                    break;
                }
            }
        }

        private async Task<DocumentDto> ParseDocumentAsync(XmlReader reader)
        {
            var document = new DocumentDto()
            {
                Sender = new MarketParticipantDto(),
                Recipient = new MarketParticipantDto(),
                RequestDate = _clock.GetCurrentInstant(),
            };

            await ParseFieldsAsync(reader, document).ConfigureAwait(false);

            return document;
        }
    }
}
