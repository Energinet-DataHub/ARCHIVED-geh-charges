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
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Dtos.MeteringPointCreatedEvents;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.MeteringPoints.Handlers
{
    public class MeteringPointPersister : IMeteringPointPersister
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public MeteringPointPersister(
            IMeteringPointRepository meteringPointRepository,
            ILoggerFactory loggerFactory,
            IUnitOfWork unitOfWork)
        {
            _meteringPointRepository = meteringPointRepository;
            _unitOfWork = unitOfWork;
            _logger = loggerFactory.CreateLogger(nameof(MeteringPointPersister));
        }

        public async Task PersistAsync(MeteringPointCreatedEvent meteringPointCreatedEvent)
        {
            ArgumentNullException.ThrowIfNull(meteringPointCreatedEvent);

            var meteringPoint = MeteringPointFactory.Create(meteringPointCreatedEvent);

            var existingMeteringPoint = await _meteringPointRepository
                .GetOrNullAsync(meteringPoint.MeteringPointId)
                .ConfigureAwait(false);

            if (existingMeteringPoint == null)
            {
                await _meteringPointRepository.AddAsync(meteringPoint).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
                _logger.LogInformation($"Metering Point ID '{meteringPoint.MeteringPointId}' has been persisted");
            }
            else
            {
                _logger.LogInformation(
                    $"Metering Point ID '{meteringPoint.MeteringPointId}' already exists in storage");

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

            if (!meteringPoint.HasSameGridAreaLinkId(existingMeteringPoint))
                _logger.LogError($"Received 'grid area link id' event data '{meteringPoint.GridAreaLinkId}' was not equal to the already persisted value '{existingMeteringPoint.GridAreaLinkId}' for Metering Point ID '{meteringPoint.MeteringPointId}'");
        }
    }
}
