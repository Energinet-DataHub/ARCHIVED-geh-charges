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

using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Application.Common.Services;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using GreenEnergyHub.Charges.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargePriceCommandReceiverConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<OutboxMessageFactory>();
            serviceCollection.AddScoped<IChargePriceCommandReceivedEventHandler, ChargePriceCommandReceivedEventHandler>();
            serviceCollection.AddScoped<IChargePriceEventHandler, ChargePriceEventHandler>();
            serviceCollection
                .AddScoped<IInputValidator<ChargePriceOperationDto>, InputValidator<ChargePriceOperationDto>>();
            serviceCollection.AddScoped<IInputValidationRulesFactory<ChargePriceOperationDto>,
                ChargePriceOperationInputValidationRulesFactory>();
            serviceCollection.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
            serviceCollection.AddScoped<IPriceConfirmedEventFactory, PriceConfirmedEventFactory>();
            serviceCollection.AddScoped<IPriceRejectedEventFactory, PriceRejectedEventFactory>();
            serviceCollection.AddScoped<JsonMessageDeserializer<ChargePriceCommandReceivedEvent>>();
        }
    }
}
