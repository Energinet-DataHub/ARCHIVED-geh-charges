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

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Bundling
{
    public class BundleCreator<TAvailableData> : IBundleCreator
        where TAvailableData : AvailableDataBase
    {
        private readonly IAvailableDataRepository<TAvailableData> _availableDataRepository;
        private readonly ICimSerializer<TAvailableData> _cimSerializer;
        private readonly ICimJsonSerializer<TAvailableData> _cimJsonSerializer;
        private readonly IStorageHandler _storageHandler;

        public BundleCreator(
            IAvailableDataRepository<TAvailableData> availableDataRepository,
            ICimSerializer<TAvailableData> cimSerializer,
            ICimJsonSerializer<TAvailableData> cimJsonSerializer,
            IStorageHandler storageHandler)
        {
            _availableDataRepository = availableDataRepository;
            _cimSerializer = cimSerializer;
            _cimJsonSerializer = cimJsonSerializer;
            _storageHandler = storageHandler;
        }

        public async Task CreateAsync(DataBundleRequestDto request, Stream outputStream)
        {
            var dataAvailableNotificationIds = await _storageHandler.GetDataAvailableNotificationIdsAsync(request).ConfigureAwait(false);

            var availableData = await _availableDataRepository
                .GetAsync(dataAvailableNotificationIds)
                .ConfigureAwait(false);

            if (availableData.Count == 0)
            {
                var ids = string.Join(",", dataAvailableNotificationIds);
                throw new UnknownDataAvailableNotificationIdsException(
                    $"Peek request with id '{request.RequestId}' resulted in available ids '{ids}' but no data was found in the database");
            }

            var firstData = availableData.First();
            if (request.ResponseFormat == ResponseFormat.Json)
            {
                await _cimJsonSerializer.SerializeToStreamAsync(
                    availableData,
                    outputStream,
                    firstData.BusinessReasonCode,
                    firstData.SenderId,
                    firstData.SenderRole,
                    firstData.RecipientId,
                    firstData.RecipientRole).ConfigureAwait(false);
            }
            else
            {
                await _cimSerializer.SerializeToStreamAsync(
                    availableData,
                    outputStream,
                    // Due to the nature of the interface to the MessageHub and the use of MessageType in that
                    // BusinessReasonCode, SenderId, SenderRole, RecipientId, RecipientRole and ReceiptStatus will always
                    // be the same value on all records in the list. We can simply take it from the first record.
                    firstData.BusinessReasonCode,
                    firstData.SenderId,
                    firstData.SenderRole,
                    firstData.RecipientId,
                    firstData.RecipientRole).ConfigureAwait(false);
            }
        }
    }
}
