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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.Charges.Services
{
    public class ChargePriceNotificationService : IChargePriceNotificationService
    {
        private readonly ILogger _logger;

        public ChargePriceNotificationService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(ChargePriceConfirmationService));
        }

        public Task SaveNotificationsAsync(List<ChargePriceOperationDto> priceOperationNotifications)
        {
            _logger.LogInformation($"{priceOperationNotifications.Count} notifications was persisted.");
            return Task.CompletedTask;
        }
    }
}
