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
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Charges.Repositories;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using NodaTime;

using DBDefaultChargeLinkSetting = GreenEnergyHub.Charges.Infrastructure.Context.Model.DefaultChargeLinkSetting;

namespace GreenEnergyHub.Charges.Infrastructure.Repositories
{
    public class DefaultChargeLinkSettingRepository : IDefaultChargeLinkSettingRepository
    {
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        public DefaultChargeLinkSettingRepository(IChargesDatabaseContext chargesDatabaseContext)
        {
            _chargesDatabaseContext = chargesDatabaseContext;
        }

        public async Task<IEnumerable<DefaultChargeLink>> GetDefaultChargeLinkSettingAsync(MeteringPointType meteringPointType)
        {
            var defaultChargeLinkSettings = await _chargesDatabaseContext.DefaultChargeLinkSettings
                .Where(x => x.MeteringPointType == (int)meteringPointType).ToListAsync().ConfigureAwait(false);

            return defaultChargeLinkSettings.Select(Map).ToList();
        }

        private static DefaultChargeLink Map(DBDefaultChargeLinkSetting defaultChargeLinkSettings)
        {
            return new DefaultChargeLink
            {
                ApplicableDate = Instant.FromDateTimeUtc(defaultChargeLinkSettings.StartDateTime.ToUniversalTime()),
                EndDate = defaultChargeLinkSettings.EndDateTime != null ?
                    Instant.FromDateTimeUtc(defaultChargeLinkSettings.EndDateTime.Value.ToUniversalTime()) :
                    Instant.MinValue,
                ChargeRowId = defaultChargeLinkSettings.ChargeRowId,
            };
        }
    }
}
