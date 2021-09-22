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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Context.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.Repositories
{
    public class MeteringPointRepository : IMeteringPointRepository
    {
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        public MeteringPointRepository(IChargesDatabaseContext chargesDatabaseContext)
        {
            _chargesDatabaseContext = chargesDatabaseContext;
        }

        public async Task StoreMeteringPointAsync(MeteringPoint meteringPoint)
        {
            if (meteringPoint == null) throw new ArgumentNullException(nameof(meteringPoint));
            var entityModel = MeteringPointMapper.MapMeteringPointToEntity(meteringPoint);
            await _chargesDatabaseContext.MeteringPoints.AddAsync(entityModel).ConfigureAwait(false);
            await _chargesDatabaseContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<MeteringPoint> GetMeteringPointAsync(string meteringPointId)
        {
            var meteringPoint = await _chargesDatabaseContext
                .MeteringPoints
                .SingleAsync(x => x.MeteringPointId == meteringPointId)
                .ConfigureAwait(false);

            return MeteringPointMapper.MapMeteringPointToDomainModel(meteringPoint);
        }
    }
}
