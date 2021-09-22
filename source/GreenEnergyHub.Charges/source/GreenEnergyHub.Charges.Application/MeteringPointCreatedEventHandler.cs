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
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Application.Mapping;
using GreenEnergyHub.Charges.Domain.Charges.Events.Integration;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application
{
    public class MeteringPointCreatedEventHandler : IMeteringPointCreatedEventHandler
    {
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly ILogger _logger;

        public MeteringPointCreatedEventHandler(
            IMeteringPointRepository meteringPointRepository,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _meteringPointRepository = meteringPointRepository;
            _logger = loggerFactory.CreateLogger(nameof(MeteringPointCreatedEventHandler));
        }

        public async Task HandleAsync(MeteringPointCreatedEvent meteringPointCreatedEvent)
        {
            if (meteringPointCreatedEvent == null)
                throw new ArgumentNullException(nameof(meteringPointCreatedEvent));

            var meteringPoint = MeteringPointMapper.MapMeteringPointCreatedEventToMeteringPoint(meteringPointCreatedEvent);
            await _meteringPointRepository.StoreMeteringPointAsync(meteringPoint).ConfigureAwait(false);
            _logger.LogInformation("Finished persisting metering point with id: {meteringPointId}", meteringPoint.MeteringPointId);
        }
    }
}
