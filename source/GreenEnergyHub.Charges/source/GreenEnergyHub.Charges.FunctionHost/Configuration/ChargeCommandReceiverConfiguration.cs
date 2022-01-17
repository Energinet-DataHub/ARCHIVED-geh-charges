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

using Energinet.DataHub.Core.Messaging.Protobuf;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Core.Currency;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Iso8601;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeCommandReceiverConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargeCommandReceiptService, ChargeCommandReceiptService>();
            serviceCollection.AddScoped<IChargeCommandReceivedEventHandler, ChargeCommandReceivedEventHandler>();
            serviceCollection.AddScoped<IChargeFactory, ChargeFactory>();
            serviceCollection.AddScoped<IChargeCommandAcceptedEventFactory, ChargeCommandAcceptedEventFactory>();
            serviceCollection.AddScoped<IChargeCommandRejectedEventFactory, ChargeCommandRejectedEventFactory>();
            serviceCollection.AddScoped<ICimValidationErrorTextFactory<ChargeCommand>, ChargeCimValidationErrorTextFactory>();
            serviceCollection.AddScoped<ICimValidationErrorCodeFactory, CimValidationErrorCodeFactory>();

            serviceCollection
                .AddScoped<IAvailableChargeReceiptValidationErrorFactory,
                    AvailableChargeReceiptValidationErrorFactory>();

            ConfigureDatabase(serviceCollection);
            ConfigureValidation(serviceCollection);
            ConfigureIso8601Timezones(serviceCollection);
            ConfigureIso4217Currency(serviceCollection);
            ConfigureMessaging(serviceCollection);
        }

        private static void ConfigureDatabase(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IMarketParticipantRepository, MarketParticipantRepository>();
        }

        private static void ConfigureValidation(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IBusinessValidationRulesFactory<ChargeCommand>, ChargeCommandBusinessValidationRulesFactory>();
            serviceCollection.AddScoped<IInputValidationRulesFactory<ChargeCommand>, ChargeCommandInputValidationRulesFactory>();
            serviceCollection.AddScoped<IRulesConfigurationRepository, RulesConfigurationRepository>();
            serviceCollection.AddScoped<IInputValidator<ChargeCommand>, InputValidator<ChargeCommand>>();
            serviceCollection.AddScoped<IBusinessValidator<ChargeCommand>, BusinessValidator<ChargeCommand>>();
            serviceCollection.AddScoped<IValidator<ChargeCommand>, Validator<ChargeCommand>>();
        }

        private static void ConfigureIso8601Timezones(IServiceCollection serviceCollection)
        {
            var timeZoneId = EnvironmentHelper.GetEnv(EnvironmentSettingNames.LocalTimeZoneName);
            var timeZoneConfiguration = new Iso8601ConversionConfiguration(timeZoneId);
            serviceCollection.AddSingleton<IIso8601ConversionConfiguration>(timeZoneConfiguration);
            serviceCollection.AddScoped<IZonedDateTimeService, ZonedDateTimeService>();
        }

        private static void ConfigureIso4217Currency(IServiceCollection serviceCollection)
        {
            var currency = EnvironmentHelper.GetEnv(EnvironmentSettingNames.Currency);
            var iso4217Currency = new CurrencyConfigurationIso4217(currency);
            serviceCollection.AddSingleton(iso4217Currency);
        }

        private static void ConfigureMessaging(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddMessaging()
                .AddMessageExtractor<ChargeCommandReceivedEvent>()
                .AddMessageDispatcher<ChargeCommandAcceptedEvent>(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedTopicName))
                .AddMessageDispatcher<ChargeCommandRejectedEvent>(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandRejectedTopicName));

            // serviceCollection.ReceiveProtobufMessage<ChargeCommandReceivedContract>(
            //     configuration => configuration.WithParser(() => ChargeCommandReceivedContract.Parser));
            //
            //
            // serviceCollection.SendProtobuf<ChargeCommandAcceptedContract>();
            // serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeCommandAcceptedEvent>(
            //     EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
            //     EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedTopicName));
            //
            // serviceCollection.SendProtobuf<ChargeCommandRejectedContract>();
            // serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeCommandRejectedEvent>(
            //     EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
            //     EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandRejectedTopicName));
        }
    }
}
