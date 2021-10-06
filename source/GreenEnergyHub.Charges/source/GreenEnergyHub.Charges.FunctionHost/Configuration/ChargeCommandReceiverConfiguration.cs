﻿// Copyright 2020 Energinet DataHub A/S
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

using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Core.Currency;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation.BusinessValidation.Factories;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation.InputValidation;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Context.Mapping;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.Messaging.Protobuf;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeCommandReceiverConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargeCommandConfirmationService, ChargeCommandConfirmationService>();
            serviceCollection.AddScoped<IChargeCommandReceivedEventHandler, ChargeCommandReceivedEventHandler>();
            serviceCollection.AddScoped<IChargeFactory, ChargeFactory>();
            serviceCollection.AddScoped<IChargeCommandFactory, ChargeCommandFactory>();
            serviceCollection.AddScoped<IChargeCommandAcceptedEventFactory, ChargeCommandAcceptedEventFactory>();
            serviceCollection.AddScoped<IChargeCommandRejectedEventFactory, ChargeCommandRejectedEventFactory>();

            ConfigureDatabase(serviceCollection);
            ConfigureValidation(serviceCollection);
            ConfigureIso8601Timezones(serviceCollection);
            ConfigureIso4217Currency(serviceCollection);
            ConfigureMessaging(serviceCollection);
        }

        private static void ConfigureDatabase(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IMarketParticipantRepository, MarketParticipantRepository>();
            serviceCollection.AddScoped<IMarketParticipantMapper, MarketParticipantMapper>();
        }

        private static void ConfigureValidation(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IBusinessValidationRulesFactory, BusinessValidationRulesFactory>();
            serviceCollection.AddScoped<IInputValidationRulesFactory, InputValidationRulesFactory>();
            serviceCollection.AddScoped<IRulesConfigurationRepository, RulesConfigurationRepository>();
            serviceCollection.AddScoped<IChargeCommandInputValidator, ChargeCommandInputValidator>();
            serviceCollection.AddScoped<IChargeCommandBusinessValidator, ChargeCommandBusinessValidator>();
            serviceCollection.AddScoped<IChargeCommandValidator, ChargeCommandValidator>();
        }

        private static void ConfigureIso8601Timezones(IServiceCollection serviceCollection)
        {
            var timeZoneId = EnvironmentHelper.GetEnv("LOCAL_TIMEZONENAME");
            var timeZoneConfiguration = new Iso8601ConversionConfiguration(timeZoneId);
            serviceCollection.AddSingleton<IIso8601ConversionConfiguration>(timeZoneConfiguration);
            serviceCollection.AddScoped<IZonedDateTimeService, ZonedDateTimeService>();
        }

        private static void ConfigureIso4217Currency(IServiceCollection serviceCollection)
        {
            var currency = EnvironmentHelper.GetEnv("CURRENCY");
            var iso4217Currency = new CurrencyConfigurationIso4217(currency);
            serviceCollection.AddSingleton(iso4217Currency);
        }

        private static void ConfigureMessaging(IServiceCollection serviceCollection)
        {
            serviceCollection.ReceiveProtobufMessage<ChargeCommandReceivedContract>(
                configuration => configuration.WithParser(() => ChargeCommandReceivedContract.Parser));

            serviceCollection.SendProtobuf<ChargeCommandAcceptedContract>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeCommandAcceptedEvent>(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                EnvironmentHelper.GetEnv("COMMAND_ACCEPTED_TOPIC_NAME"));

            serviceCollection.SendProtobuf<ChargeCommandRejectedContract>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeCommandRejectedEvent>(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                EnvironmentHelper.GetEnv("COMMAND_REJECTED_TOPIC_NAME"));
        }
    }
}
