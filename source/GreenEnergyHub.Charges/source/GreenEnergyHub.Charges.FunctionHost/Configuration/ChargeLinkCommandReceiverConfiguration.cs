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

using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Application.ChargeLinks.Services;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.InputValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration;
using GreenEnergyHub.Charges.Infrastructure.Core.Registration;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeLinkCommandReceiverConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargeLinksReceivedEventHandler, ChargeLinksReceivedEventHandler>();
            serviceCollection.AddScoped<IChargeLinkFactory, ChargeLinkFactory>();
            serviceCollection.AddSingleton<IChargeLinksAcceptedEventFactory, ChargeLinksAcceptedEventFactory>();
            serviceCollection.AddScoped<IChargeLinksRepository, ChargeLinksRepository>();
            serviceCollection.AddScoped<IBusinessValidationRulesFactory<ChargeLinksCommand>,
                ChargeLinksCommandBusinessValidationRulesFactory>();
            serviceCollection.AddScoped<IBusinessValidator<ChargeLinksCommand>,
                BusinessValidator<ChargeLinksCommand>>();
            serviceCollection.AddScoped<IChargeLinksReceiptService, ChargeLinksReceiptService>();
            serviceCollection.AddScoped<IChargeLinksRejectedEventFactory, ChargeLinksRejectedEventFactory>();
            serviceCollection.AddScoped<IBusinessValidator<ChargeLinksCommand>,
                BusinessValidator<ChargeLinksCommand>>();
            serviceCollection.AddScoped<IBusinessValidationRulesFactory<ChargeLinksCommand>,
                ChargeLinksCommandBusinessValidationRulesFactory>();
            serviceCollection.AddScoped<IInputValidationRulesFactory<ChargeLinksCommand>,
                ChargeLinksCommandInputValidationRulesFactory>();
            serviceCollection.AddScoped<IValidator<ChargeLinksCommand>, Validator<ChargeLinksCommand>>();
            serviceCollection.AddScoped<IInputValidator<ChargeLinksCommand>, InputValidator<ChargeLinksCommand>>();
            serviceCollection.AddScoped<IAvailableChargeLinksReceiptValidationErrorFactory,
                AvailableChargeLinksReceiptValidationErrorFactory>();
            serviceCollection.AddScoped<ICimValidationErrorTextFactory<ChargeLinksCommand, ChargeLinkDto>,
                ChargeLinksCimValidationErrorTextFactory>();

            serviceCollection.AddMessaging()
                .AddInternalMessageExtractor<ChargeLinksReceivedEvent>()
                .AddInternalMessageDispatcher<ChargeLinksAcceptedEvent>(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedTopicName))
                .AddInternalMessageDispatcher<ChargeLinksRejectedEvent>(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksRejectedTopicName));
        }
    }
}
