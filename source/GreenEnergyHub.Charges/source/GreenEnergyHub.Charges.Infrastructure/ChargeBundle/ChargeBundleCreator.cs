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

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.Application.Charges.MessageHub;
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Infrastructure.ChargeBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.MessageHub;

namespace GreenEnergyHub.Charges.Infrastructure.ChargeBundle
{
    public class ChargeBundleCreator : IBundleCreator
    {
        private readonly IAvailableChargeDataRepository _availableChargeDataRepository;

        private readonly IChargeCimSerializer _chargeCimSerializer;
        private readonly IStorageHandler _storageHandler;

        public ChargeBundleCreator(
            IAvailableChargeDataRepository availableChargeDataRepository,
            IChargeCimSerializer chargeCimSerializer,
            IStorageHandler storageHandler)
        {
            _availableChargeDataRepository = availableChargeDataRepository;
            _chargeCimSerializer = chargeCimSerializer;
            _storageHandler = storageHandler;
        }

        public async Task CreateAsync(DataBundleRequestDto request, Stream outputStream)
        {
            if (!request.MessageType.StartsWith(ChargeDataAvailableNotifier.MessageTypePrefix))
                throw new InvalidOperationException($"{nameof(ChargeBundleCreator)} does not support message type '{request.MessageType}'.");

            var dataAvailableNotificationIds = await _storageHandler.GetDataAvailableNotificationIdsAsync(request).ConfigureAwait(false);

            var availableData = await _availableChargeDataRepository
                .GetAsync(dataAvailableNotificationIds)
                .ConfigureAwait(false);

            await _chargeCimSerializer.SerializeToStreamAsync(
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
