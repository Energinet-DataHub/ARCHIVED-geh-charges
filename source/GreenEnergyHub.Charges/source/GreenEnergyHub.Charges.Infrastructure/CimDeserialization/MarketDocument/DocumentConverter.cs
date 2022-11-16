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

using System;
using System.Threading.Tasks;
using Energinet.DataHub.Core.SchemaValidation;
using Energinet.DataHub.Core.SchemaValidation.Extensions;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.CimDeserialization.MarketDocument
{
    public abstract class DocumentConverter
    {
        private readonly IClock _clock;

        protected DocumentConverter(IClock clock)
        {
            _clock = clock;
        }

        public async Task<ChargeCommandBundle> ConvertAsync(SchemaValidatingReader reader)
        {
            var document = await ParseDocumentAsync(reader).ConfigureAwait(false);

            var message = await ConvertSpecializedContentAsync(reader, document).ConfigureAwait(false);

            return message;
        }

        protected abstract Task<ChargeCommandBundle> ConvertSpecializedContentAsync(SchemaValidatingReader reader, DocumentDto document);

        private async Task<DocumentDto> ParseFieldsAsync(SchemaValidatingReader reader)
        {
            var hasReadRoot = false;

            var id = string.Empty;
            var type = default(DocumentType);
            var businessReasonCode = default(BusinessReasonCode);
            var industryClassification = default(IndustryClassification);
            var senderMarketParticipantId = string.Empty;
            var senderBusinessProcessRole = default(MarketParticipantRole);
            var recipientMarketParticipantId = string.Empty;
            var recipientBusinessProcessRole = default(MarketParticipantRole);
            var createdDateTime = default(Instant);

            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (!hasReadRoot)
                {
                    hasReadRoot = true;
                }
                else if (reader.Is(CimMarketDocumentConstants.Id))
                {
                    ValidateElementContent(reader);
                    id = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                }
                else if (reader.Is(CimMarketDocumentConstants.Type))
                {
                    ValidateElementContent(reader);
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    type = DocumentTypeMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.BusinessReasonCode))
                {
                    if (!reader.CanReadValue) continue;
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    businessReasonCode = BusinessReasonCodeMapper.Map(content);
                    ValidateBusinessReasonCode(businessReasonCode, content);
                }
                else if (reader.Is(CimMarketDocumentConstants.IndustryClassification))
                {
                    if (!reader.CanReadValue) continue;
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    industryClassification = IndustryClassificationMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.SenderId))
                {
                    if (!reader.CanReadValue) continue;
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    senderMarketParticipantId = content;
                }
                else if (reader.Is(CimMarketDocumentConstants.SenderBusinessProcessRole))
                {
                    if (!reader.CanReadValue) continue;
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    senderBusinessProcessRole = MarketParticipantRoleMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.RecipientId))
                {
                    if (!reader.CanReadValue) continue;
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    recipientMarketParticipantId = content;
                }
                else if (reader.Is(CimMarketDocumentConstants.RecipientBusinessProcessRole))
                {
                    if (!reader.CanReadValue) continue;
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    recipientBusinessProcessRole = MarketParticipantRoleMapper.Map(content);
                }
                else if (reader.Is(CimMarketDocumentConstants.CreatedDateTime))
                {
                    if (!reader.CanReadValue) continue;
                    createdDateTime = await reader.ReadValueAsNodaTimeAsync().ConfigureAwait(false);
                }
                else if (reader.IsElement())
                {
                    // CIM does not have the payload in a separate element,
                    // so we have to assume that the first unknown element
                    // is the start of the specialized document
                    break;
                }
            }

            return new DocumentDto(
                id,
                _clock.GetCurrentInstant(),
                type,
                createdDateTime,
                new MarketParticipantDto(Guid.NewGuid(), senderMarketParticipantId, senderBusinessProcessRole, Guid.Empty),
                new MarketParticipantDto(Guid.NewGuid(), recipientMarketParticipantId, recipientBusinessProcessRole, Guid.Empty),
                industryClassification,
                businessReasonCode);
        }

        private static void ValidateBusinessReasonCode(BusinessReasonCode businessReasonCode, string content)
        {
            if (businessReasonCode == BusinessReasonCode.Unknown)
            {
                throw new InvalidXmlValueException(
                    B2BErrorCode.CouldNotMapEnumErrorMessage,
                    CimMarketDocumentConstants.BusinessReasonCode,
                    content);
            }
        }

        private static void ValidateElementContent(ISchemaValidatingReader reader)
        {
            if (!reader.CanReadValue)
            {
                throw new InvalidXmlValueException(
                    B2BErrorCode.IsEmptyOrWhitespaceErrorMessage,
                    reader.CurrentNodeName,
                    string.Empty);
            }
        }

        private async Task<DocumentDto> ParseDocumentAsync(SchemaValidatingReader reader)
        {
            var document = await ParseFieldsAsync(reader).ConfigureAwait(false);

            if (reader.HasErrors) throw new SchemaValidationException(reader.CreateErrorResponse());

            return document;
        }
    }
}
