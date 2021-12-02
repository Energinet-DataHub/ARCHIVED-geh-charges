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

using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Core.Currency;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Context.Mapping;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Iso8601;
using SimpleInjector;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeCommandReceiverConfiguration
    {
        internal static void ConfigureServices(Container container)
        {
            container.Register<IChargeCommandConfirmationService, ChargeCommandConfirmationService>(Lifestyle.Scoped);
            container.Register<IChargeCommandReceivedEventHandler, ChargeCommandReceivedEventHandler>(Lifestyle.Scoped);
            container.Register<IChargeFactory, ChargeFactory>(Lifestyle.Scoped);
            container.Register<IChargeCommandFactory, ChargeCommandFactory>(Lifestyle.Scoped);
            container.Register<IChargeCommandAcceptedEventFactory, ChargeCommandAcceptedEventFactory>(Lifestyle.Scoped);
            container.Register<IChargeCommandRejectedEventFactory, ChargeCommandRejectedEventFactory>(Lifestyle.Scoped);

            ConfigureDatabase(container);
            ConfigureValidation(container);
            ConfigureIso8601Timezones(container);
            ConfigureIso4217Currency(container);
            ConfigureMessaging(container);
        }

        private static void ConfigureDatabase(Container container)
        {
            container.Register<IMarketParticipantRepository, MarketParticipantRepository>(Lifestyle.Scoped);
            container.Register<IMarketParticipantMapper, MarketParticipantMapper>(Lifestyle.Scoped);
        }

        private static void ConfigureValidation(Container serviceCollection)
        {
            serviceCollection.Register<IBusinessValidationRulesFactory, BusinessValidationRulesFactory>(Lifestyle.Scoped);
            serviceCollection.Register<IInputValidationRulesFactory, InputValidationRulesFactory>(Lifestyle.Scoped);
            serviceCollection.Register<IRulesConfigurationRepository, RulesConfigurationRepository>(Lifestyle.Scoped);
            serviceCollection.Register<IChargeCommandInputValidator, ChargeCommandInputValidator>(Lifestyle.Scoped);
            serviceCollection.Register<IChargeCommandBusinessValidator, ChargeCommandBusinessValidator>(Lifestyle.Scoped);
            serviceCollection.Register<IChargeCommandValidator, ChargeCommandValidator>(Lifestyle.Scoped);
        }

        private static void ConfigureIso8601Timezones(Container serviceCollection)
        {
            var timeZoneId = EnvironmentHelper.GetEnv(EnvironmentSettingNames.LocalTimeZoneName);
            serviceCollection.Register<IIso8601ConversionConfiguration>(() => new Iso8601ConversionConfiguration(timeZoneId));
            serviceCollection.Register<IZonedDateTimeService, ZonedDateTimeService>(Lifestyle.Scoped);
        }

        private static void ConfigureIso4217Currency(Container container)
        {
            var currency = EnvironmentHelper.GetEnv(EnvironmentSettingNames.Currency);
            container.Register(() => new CurrencyConfigurationIso4217(currency));
        }

        private static void ConfigureMessaging(Container container)
        {
            container.ReceiveProtobufMessage<ChargeCommandReceivedContract>(
                configuration => configuration.WithParser(() => ChargeCommandReceivedContract.Parser));

            container.SendProtobufMessage<ChargeCommandAcceptedContract>();
            container.AddMessagingProtobuf().AddMessageDispatcher<ChargeCommandAcceptedEvent>(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedTopicName));

            container.SendProtobufMessage<ChargeCommandRejectedContract>();
            container.AddMessagingProtobuf().AddMessageDispatcher<ChargeCommandRejectedEvent>(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandRejectedTopicName));
        }
    }
}
