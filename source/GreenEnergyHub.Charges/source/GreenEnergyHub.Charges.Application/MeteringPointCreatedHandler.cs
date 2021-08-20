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
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Domain.Events.Integration;

namespace GreenEnergyHub.Charges.Application
{
    public class MeteringPointCreatedHandler : IMeteringPointCreatedHandler
    {
        private readonly IMeteringPointRepository _meteringPointRepository;

        public MeteringPointCreatedHandler(IMeteringPointRepository meteringPointRepository)
        {
            _meteringPointRepository = meteringPointRepository;
        }

        public async Task HandleAsync(MeteringPointCreatedEvent meteringPointCreatedEvent)
        {
            if (meteringPointCreatedEvent == null)
            {
                throw new ArgumentNullException(nameof(meteringPointCreatedEvent));
            }

            await _meteringPointRepository.StoreMeteringPointAsync(meteringPointCreatedEvent).ConfigureAwait(false);
        }
    }
}
