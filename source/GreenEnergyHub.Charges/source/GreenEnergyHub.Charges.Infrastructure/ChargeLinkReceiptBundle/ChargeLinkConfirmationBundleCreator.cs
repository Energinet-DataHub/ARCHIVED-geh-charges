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
using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.Infrastructure.ChargeLinkReceiptBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.MessageHub;

namespace GreenEnergyHub.Charges.Infrastructure.ChargeLinkReceiptBundle
{
    public class ChargeLinkConfirmationBundleCreator : IBundleCreator
    {
        private readonly IAvailableChargeLinkReceiptDataRepository _availableChargeLinkReceiptDataRepository;
        private readonly IChargeLinkReceiptCimSerializer _chargeLinkReceiptCimSerializer;
        private readonly IStorageHandler _storageHandler;

        public ChargeLinkConfirmationBundleCreator(
            IAvailableChargeLinkReceiptDataRepository availableChargeLinkReceiptDataRepository,
            IChargeLinkReceiptCimSerializer chargeLinkReceiptCimSerializer,
            IStorageHandler storageHandler)
        {
            _availableChargeLinkReceiptDataRepository = availableChargeLinkReceiptDataRepository;
            _chargeLinkReceiptCimSerializer = chargeLinkReceiptCimSerializer;
            _storageHandler = storageHandler;
        }

        public async Task CreateAsync(DataBundleRequestDto request, Stream outputStream)
        {
            var dataAvailableNotificationIds = await _storageHandler.GetDataAvailableNotificationIdsAsync(request).ConfigureAwait(false);

            var availableData = await _availableChargeLinkReceiptDataRepository
                .GetAsync(dataAvailableNotificationIds)
                .ConfigureAwait(false);

            await _chargeLinkReceiptCimSerializer.SerializeToStreamAsync(
                availableData,
                outputStream,
                // Due to the nature of the interface to the MessageHub and the use of MessageType in that
                // BusinessReasonCode, RecipientId, RecipientRole and ReceiptStatus will always be the same value
                // on all records in the list. We can simply take it from the first record.
                availableData.First().BusinessReasonCode,
                availableData.First().RecipientId,
                availableData.First().RecipientRole).ConfigureAwait(false);
        }
    }
}
