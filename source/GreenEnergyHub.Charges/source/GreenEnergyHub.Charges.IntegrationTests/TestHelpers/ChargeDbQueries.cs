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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public class ChargeDbQueries
    {
        private readonly ServiceProvider _serviceProvider;

        public ChargeDbQueries(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<bool> ChargeExistsAsync(
            [NotNull] string chargeId, [NotNull] string owner, [NotNull] ChargeType chargeType)
        {
            await using var context = _serviceProvider.GetService<ChargesDatabaseContext>();
            var chargeRepository = new ChargeRepository(context);
            var chargeExists = await chargeRepository
                .CheckIfChargeExistsAsync(chargeId, owner, chargeType)
                .ConfigureAwait(false);
            return chargeExists;
        }

        public async Task<Charge> GetChargeAsync(
            [NotNull] string chargeId, [NotNull] string owner, [NotNull] ChargeType chargeType)
        {
            await using var context = _serviceProvider.GetService<ChargesDatabaseContext>();
            var chargeRepository = new ChargeRepository(context);
            var chargeExists = await chargeRepository
                .GetChargeAsync(chargeId, owner, chargeType)
                .ConfigureAwait(false);
            return chargeExists;
        }
    }
}
