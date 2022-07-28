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

using GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.AvailableData.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableChargeReceiptData
{
    public class AvailableChargeReceiptDataRepository : IAvailableDataRepository<GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableChargeReceiptData.AvailableChargeReceiptData>
    {
        private readonly IMessageHubDatabaseContext _context;

        public AvailableChargeReceiptDataRepository(IMessageHubDatabaseContext context)
        {
            _context = context;
        }

        public async Task StoreAsync(IEnumerable<GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableChargeReceiptData.AvailableChargeReceiptData> availableChargeReceiptData)
        {
            await _context.AvailableChargeReceiptData.AddRangeAsync(availableChargeReceiptData).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableChargeReceiptData.AvailableChargeReceiptData>> GetAsync(IEnumerable<Guid> dataReferenceIds)
        {
            return await _context
                .AvailableChargeReceiptData
                .Where(x => dataReferenceIds.Contains(x.AvailableDataReferenceId))
                .OrderBy(x => x.RequestDateTime)
                .ToListAsync()
                .ConfigureAwait(false);
        }
    }
}
