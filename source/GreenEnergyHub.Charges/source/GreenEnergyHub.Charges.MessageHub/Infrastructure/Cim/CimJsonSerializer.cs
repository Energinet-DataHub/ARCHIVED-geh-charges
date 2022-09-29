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
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim
{
    public abstract class CimJsonSerializer<T> : ICimJsonSerializer<T>
        where T : AvailableDataBase
    {
        public CimJsonSerializer(IClock clock, ICimIdProvider cimIdProvider)
        {
            Clock = clock;
            CimIdProvider = cimIdProvider;
        }

        public IClock Clock { get; }

        public ICimIdProvider CimIdProvider { get; }

        public async Task SerializeToStreamAsync(
            IEnumerable<T> availableData,
            Stream stream,
            BusinessReasonCode businessReasonCode,
            string senderId,
            MarketParticipantRole senderRole,
            string recipientId,
            MarketParticipantRole recipientRole)
        {
            var jsonDocument = new JsonObject
            {
                {
                    GetRootElementName(),
                    GetDocument(availableData, businessReasonCode, senderId, senderRole, recipientId, recipientRole)
                },
            };
            var options = new JsonSerializerOptions { WriteIndented = true };
            var document = jsonDocument.ToJsonString(options);
            var bytes = Encoding.UTF8.GetBytes(document);
            await stream.WriteAsync(bytes, 0, bytes.Length, CancellationToken.None).ConfigureAwait(false);
            stream.Position = 0;
        }

        protected abstract string GetRootElementName();

        protected abstract DocumentType GetDocumentType();

        protected abstract JsonObject GetActivityRecord(T record);

        private JsonObject GetDocument(
            IEnumerable<T> records,
            BusinessReasonCode businessReasonCode,
            string senderId,
            MarketParticipantRole senderRole,
            string recipientId,
            MarketParticipantRole recipientRole)
        {
            var jsonContent = new JsonObject
            {
                    { CimMarketDocumentConstants.Id, CimIdProvider.GetUniqueId() },
                    {
                        CimMarketDocumentConstants.IndustryClassification,
                        CimJsonHelper.CreateValueObject(IndustryClassificationMapper.Map(IndustryClassification.Electricity))
                    },
                    { CimMarketDocumentConstants.CreatedDateTime, Clock.GetCurrentInstant().ToDateTimeUtc() },
                    {
                        CimMarketDocumentConstants.BusinessReasonCode,
                        CimJsonHelper.CreateValueObject(BusinessReasonCodeMapper.Map(businessReasonCode))
                    },
                    { CimMarketDocumentConstants.RecipientId, CimJsonHelper.CreateValueObject(recipientId, CodingScheme.GS1) },
                    {
                        CimMarketDocumentConstants.RecipientBusinessProcessRole,
                        CimJsonHelper.CreateValueObject(MarketParticipantRoleMapper.Map(recipientRole))
                    },
                    { CimMarketDocumentConstants.SenderId, CimJsonHelper.CreateValueObject(senderId, CodingScheme.GS1) },
                    {
                        CimMarketDocumentConstants.SenderBusinessProcessRole,
                        CimJsonHelper.CreateValueObject(MarketParticipantRoleMapper.Map(senderRole))
                    },
                    {
                        CimMarketDocumentConstants.Type,
                        CimJsonHelper.CreateValueObject(DocumentTypeMapper.Map(GetDocumentType()))
                    },
                    {
                        CimMarketDocumentConstants.MarketActivityRecord, GetActivityRecords(records)
                    },
            };
            return jsonContent;
        }

        private JsonArray GetActivityRecords(IEnumerable<T> records)
        {
            var marketActivityRecords = new JsonArray();
            foreach (var record in records)
            {
                marketActivityRecords.Add(GetActivityRecord(record));
            }

            return marketActivityRecords;
        }
    }
}
