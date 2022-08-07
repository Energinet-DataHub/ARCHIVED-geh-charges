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

using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Application.Charges.Services;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargePriceCommandReceiverConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargePriceCommandReceivedEventHandler, ChargePriceCommandReceivedEventHandler>();
            serviceCollection.AddScoped<IChargePriceEventHandler, ChargePriceEventHandler>();
            serviceCollection
                .AddScoped<IInputValidator<ChargePriceOperationDto>, InputValidator<ChargePriceOperationDto>>();
            ConfigureMessaging(serviceCollection);
            serviceCollection.AddScoped<IInputValidationRulesFactory<ChargePriceOperationDto>,
                ChargePriceOperationInputValidationRulesFactory>();
            serviceCollection.AddScoped<IChargePriceConfirmationService, ChargePriceConfirmationService>();
            serviceCollection.AddScoped<IChargePriceRejectionService, ChargePriceRejectionService>();
            serviceCollection.AddScoped<IChargePriceNotificationService, ChargePriceNotificationService>();
        }

        private static void ConfigureMessaging(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddMessaging()
                .AddInternalMessageExtractor<ChargePriceCommandReceivedEvent>();
        }
    }
}
