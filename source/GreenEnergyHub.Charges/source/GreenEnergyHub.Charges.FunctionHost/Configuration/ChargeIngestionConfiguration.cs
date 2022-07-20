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

using Energinet.DataHub.Core.Messaging.Transport.SchemaValidation;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.DocumentValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.CimDeserialization.ChargeBundle;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration;
using GreenEnergyHub.Charges.Infrastructure.Core.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeIngestionConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ChargeCommandBundleConverter>();
            serviceCollection.AddScoped<IHttpResponseBuilder, HttpResponseBuilder>();
            serviceCollection.AddScoped<ValidatingMessageExtractor<ChargeCommandBundle>>();
            serviceCollection.AddScoped<IChargeCommandBundleConverter, ChargeCommandBundleConverter>();
            serviceCollection.AddScoped<SchemaValidatingMessageDeserializer<ChargeCommandBundle>, ChargeCommandDeserializer>();
            serviceCollection.AddScoped<IChargeInformationCommandBundleHandler, ChargeInformationCommandBundleHandler>();
            serviceCollection.AddScoped<IChargeInformationCommandHandler, ChargeInformationCommandHandler>();
            serviceCollection.AddScoped<IChargePriceCommandBundleHandler, ChargePriceCommandBundleHandler>();
            serviceCollection.AddScoped<IChargePriceCommandHandler, ChargePriceCommandHandler>();
            serviceCollection.AddScoped<IDocumentValidationRulesFactory, ChargeCommandDocumentValidationRulesFactory>();
            serviceCollection.AddScoped<IDocumentValidator, DocumentValidator>();

            serviceCollection.AddMessaging()
                .AddInternalMessageDispatcher<ChargeCommandReceivedEvent>(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandReceivedTopicName))
                .AddInternalMessageDispatcher<ChargePriceCommandReceivedEvent>(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.PriceCommandReceivedTopicName));
        }
    }
}
