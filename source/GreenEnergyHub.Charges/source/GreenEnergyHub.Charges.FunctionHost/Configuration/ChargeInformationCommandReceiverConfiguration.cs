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
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.Core.Currency;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using GreenEnergyHub.Charges.Infrastructure.Core.Registration;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using GreenEnergyHub.Iso8601;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeInformationCommandReceiverConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargeInformationOperationsHandler, ChargeInformationOperationsHandler>();
            serviceCollection.AddScoped<IChargeFactory, ChargeFactory>();
            serviceCollection.AddScoped<IChargeIdentifierFactory, ChargeIdentifierFactory>();
            serviceCollection.AddScoped<IChargePeriodFactory, ChargePeriodFactory>();
            serviceCollection.AddScoped<IChargeInformationOperationsAcceptedEventFactory, ChargeInformationOperationsAcceptedEventFactory>();
            serviceCollection.AddScoped<IChargeInformationOperationsRejectedEventFactory, ChargeInformationOperationsRejectedEventFactory>();
            serviceCollection.AddScoped<ICimValidationErrorTextFactory<ChargeInformationOperationDto>,
                ChargeCimValidationErrorTextFactory>();
            serviceCollection.AddScoped<ICimValidationErrorCodeFactory, CimValidationErrorCodeFactory>();
            serviceCollection.AddScoped<IAvailableChargeReceiptValidationErrorFactory,
                AvailableChargeReceiptValidationErrorFactory>();
            serviceCollection.AddScoped<IChargeCommandReceivedEventHandler, ChargeInformationCommandReceivedEventHandler>();
            serviceCollection.AddScoped<JsonMessageDeserializer<ChargeInformationCommandReceivedEvent>>();
            ConfigureDatabase(serviceCollection);
            ConfigureValidation(serviceCollection);
            ConfigureIso8601Timezones(serviceCollection);
            ConfigureIso4217Currency(serviceCollection);
        }

        private static void ConfigureDatabase(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IMarketParticipantRepository, MarketParticipantRepository>();
        }

        private static void ConfigureValidation(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IInputValidationRulesFactory<ChargeInformationOperationDto>,
                ChargeOperationInputValidationRulesFactory>();
            serviceCollection.AddScoped<IInputValidator<ChargeInformationOperationDto>, InputValidator<ChargeInformationOperationDto>>();
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
    }
}
