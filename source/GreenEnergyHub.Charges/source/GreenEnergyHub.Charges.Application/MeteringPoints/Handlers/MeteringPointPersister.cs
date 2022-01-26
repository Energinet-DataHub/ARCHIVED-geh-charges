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
using GreenEnergyHub.Charges.Domain.Dtos.MeteringPointCreatedEvents;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.MeteringPoints.Handlers
{
    public class MeteringPointPersister : IMeteringPointPersister
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly ILogger _logger;

        public MeteringPointPersister(
            IMeteringPointRepository meteringPointRepository,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _meteringPointRepository = meteringPointRepository;
            _logger = loggerFactory.CreateLogger(nameof(MeteringPointPersister));
        }

        public async Task PersistAsync(MeteringPointCreatedEvent meteringPointCreatedEvent)
        {
            if (meteringPointCreatedEvent == null)
                throw new ArgumentNullException(nameof(meteringPointCreatedEvent));

            var meteringPoint = MeteringPointFactory.Create(meteringPointCreatedEvent);

            var existingMeteringPoint = await _meteringPointRepository.GetOrNullAsync(meteringPoint.MeteringPointId);

            if (existingMeteringPoint == null)
            {
                await _meteringPointRepository.StoreMeteringPointAsync(meteringPoint).ConfigureAwait(false);
                _logger.LogInformation($"Metering Point ID '{meteringPoint.MeteringPointId}' has been persisted");
            }
            else
            {
                _logger.LogInformation(
                    $"Metering Point ID '{meteringPoint.MeteringPointId}' already exists in storage");

                // TODO BJARKE: How does this method ensure that they are identical? Shouldn't it throw? At least the name is very misleading
                // Compare and log differences between the integration event data and the persisted metering point's data
                EnsureMeteringPointsAreIdentical(meteringPoint, existingMeteringPoint);
            }
        }

        /// <summary>
        /// Compares a subset of properties of two metering point domain objects
        /// </summary>
        /// <param name="meteringPoint">The Metering Point from the integration event</param>
        /// <param name="existingMeteringPoint">The Metering Point from storage</param>
        private void EnsureMeteringPointsAreIdentical(MeteringPoint meteringPoint, MeteringPoint existingMeteringPoint)
        {
            if (!meteringPoint.HasSameMeteringPointType(existingMeteringPoint))
                _logger.LogError($"Received 'metering point type' event data '{meteringPoint.MeteringPointType}' was not equal to the already persisted value '{existingMeteringPoint.MeteringPointType}' for Metering Point ID '{meteringPoint.MeteringPointId}'");

            if (!meteringPoint.HasSameSettlementMethod(existingMeteringPoint))
                _logger.LogError($"Received 'settlement method' event data '{meteringPoint.SettlementMethod}' was not equal to the already persisted value '{existingMeteringPoint.SettlementMethod}' for Metering Point ID '{meteringPoint.MeteringPointId}'");

            if (!meteringPoint.HasSameGridAreaId(existingMeteringPoint))
                _logger.LogError($"Received 'grid area link id' event data '{meteringPoint.GridAreaLinkId}' was not equal to the already persisted value '{existingMeteringPoint.GridAreaLinkId}' for Metering Point ID '{meteringPoint.MeteringPointId}'");
        }
    }
}
