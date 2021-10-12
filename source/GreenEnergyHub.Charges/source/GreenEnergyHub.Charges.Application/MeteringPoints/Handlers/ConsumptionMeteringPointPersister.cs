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

            if (await _meteringPointRepository.ExistsAsync(meteringPoint.MeteringPointId))
            {
                _logger.LogError(
                    $"Metering Point ID '{meteringPoint.MeteringPointId}' already exists in storage.");

                var existingMeteringPoint = await _meteringPointRepository.GetMeteringPointAsync(meteringPoint.MeteringPointId);

                // Compare and log differences between the integration event data and the persisted metering point's data
                CompareMeteringPoints(meteringPoint, existingMeteringPoint);
            }
            else
            {
                await _meteringPointRepository.StoreMeteringPointAsync(meteringPoint).ConfigureAwait(false);
                _logger.LogInformation($"Consumption Metering Point ID '{meteringPoint.MeteringPointId}' has been persisted");
            }
        }

        /// <summary>
        /// Compares a subset of properties of two metering point domain objects
        /// </summary>
        /// <param name="incoming">The Metering Point from the integration event</param>
        /// <param name="existing">The Metering Point from storage</param>
        private void CompareMeteringPoints(MeteringPoint incoming, MeteringPoint existing)
        {
            if (incoming.MeteringPointType != existing.MeteringPointType)
                _logger.LogError($"Received 'metering point type' event data '{incoming.MeteringPointType}' was not equal to the already persisted value '{existing.MeteringPointType}' for Metering Point ID '{incoming.MeteringPointId}'");

            if (incoming.SettlementMethod != existing.SettlementMethod)
                _logger.LogError($"Received 'settlement method' event data '{incoming.SettlementMethod}' was not equal to the already persisted value '{existing.SettlementMethod}' for Metering Point ID '{incoming.MeteringPointId}'");

            if (incoming.GridAreaId != existing.GridAreaId)
                _logger.LogError($"Received 'grid area id' event data '{incoming.GridAreaId}' was not equal to the already persisted value '{existing.GridAreaId}' for Metering Point ID '{incoming.MeteringPointId}'");
        }
    }
}
