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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.MeteringPointCreatedEvents;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.MeteringPoints.Handlers
{
    public class ConsumptionMeteringPointPersister : IConsumptionMeteringPointPersister
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly ILogger _logger;

        public ConsumptionMeteringPointPersister(
            IMeteringPointRepository meteringPointRepository,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _meteringPointRepository = meteringPointRepository;
            _logger = loggerFactory.CreateLogger(nameof(ConsumptionMeteringPointPersister));
        }

        public async Task PersistAsync(ConsumptionMeteringPointCreatedEvent consumptionMeteringPointCreatedEvent)
        {
            if (consumptionMeteringPointCreatedEvent == null)
                throw new ArgumentNullException(nameof(consumptionMeteringPointCreatedEvent));

            var meteringPoint = MeteringPointFactory.Create(consumptionMeteringPointCreatedEvent);
            await _meteringPointRepository.StoreMeteringPointAsync(meteringPoint).ConfigureAwait(false);
            _logger.LogInformation("Finished persisting metering point with id: {meteringPointId}", meteringPoint.MeteringPointId);
        }
    }
}
