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
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.Acknowledgement;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Application.Factories;
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Factories;
using GreenEnergyHub.Charges.Application.Validation.InputValidation;
using GreenEnergyHub.Charges.ChargeCommandReceiver;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Context.Mapping;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.Infrastructure.Mapping;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.Messaging;
using GreenEnergyHub.Messaging.Protobuf;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GreenEnergyHub.Charges.ChargeCommandReceiver
{
    public class Startup : FunctionsStartup
    {
        public override void Configure([NotNull] IFunctionsHostBuilder builder)
        {
            builder.Services.AddGreenEnergyHub(typeof(ChangeOfChargesMessageHandler).Assembly);
            builder.Services.AddScoped(typeof(IClock), _ => SystemClock.Instance);
            builder.Services.AddScoped<IChangeOfChargesTransactionHandler, ChangeOfChargesTransactionHandler>();
            builder.Services.AddScoped<IChargeCommandConfirmationService, ChargeCommandConfirmationService>();
            builder.Services.AddScoped<IChargeCommandHandler, ChargeCommandHandler>();
            builder.Services.AddScoped<IChargeFactory, ChargeFactory>();
            builder.Services.AddScoped<IChargeCommandAcceptedEventFactory, ChargeCommandAcceptedEventFactory>();
            builder.Services.AddScoped<IChargeCommandRejectedEventFactory, ChargeCommandRejectedEventFactory>();

            ConfigurePersistence(builder);
            ConfigureValidation(builder);
            ConfigureIso8601Services(builder.Services);
            ConfigureMessaging(builder);
        }

        protected virtual void ConfigureMessaging([NotNull] IFunctionsHostBuilder builder)
        {
            builder.Services.ReceiveProtobuf<ChargeCommandReceivedContract>(
                configuration => configuration.WithParser(() => ChargeCommandReceivedContract.Parser));

            builder.Services.SendProtobuf<ChargeCommandAcceptedContract>();
            builder.Services.AddMessagingProtobuf().AddMessageDispatcher<ChargeCommandAcceptedEvent>(
                GetEnv("COMMAND_ACCEPTED_SENDER_CONNECTION_STRING"),
                GetEnv("COMMAND_ACCEPTED_TOPIC_NAME"));

            builder.Services.SendProtobuf<ChargeCommandRejectedContract>();
            builder.Services.AddMessagingProtobuf().AddMessageDispatcher<ChargeCommandRejectedEvent>(
                GetEnv("COMMAND_REJECTED_SENDER_CONNECTION_STRING"),
                GetEnv("COMMAND_REJECTED_TOPIC_NAME"));
        }

        protected virtual void ConfigurePersistence([NotNull] IFunctionsHostBuilder builder)
        {
            var connectionString = Environment.GetEnvironmentVariable("CHARGE_DB_CONNECTION_STRING") ??
                                   throw new ArgumentNullException(
                                       "CHARGE_DB_CONNECTION_STRING",
                                       "does not exist in configuration settings");
            builder.Services.AddDbContext<ChargesDatabaseContext>(
                options => options.UseSqlServer(connectionString));
            builder.Services.AddScoped<IChargesDatabaseContext, ChargesDatabaseContext>();
            builder.Services.AddScoped<IChargeRepository, ChargeRepository>();
            builder.Services.AddScoped<IMarketParticipantRepository, MarketParticipantRepository>();
            builder.Services.AddScoped<IMarketParticipantMapper, MarketParticipantMapper>();
        }

        protected virtual void ConfigureIso8601Services(IServiceCollection services)
        {
            const string timeZoneIdString = "LOCAL_TIMEZONENAME";
            var timeZoneId = Environment.GetEnvironmentVariable(timeZoneIdString) ??
                             throw new ArgumentNullException(
                                 timeZoneIdString,
                                 "does not exist in configuration settings");
            var timeZoneConfiguration = new Iso8601ConversionConfiguration(timeZoneId);
            services.AddSingleton<IIso8601ConversionConfiguration>(timeZoneConfiguration);
            services.AddScoped<IZonedDateTimeService, ZonedDateTimeService>();
        }

        private static void ConfigureValidation(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<IBusinessCreateValidationRulesFactory, BusinessCreateValidationRulesFactory>();
            builder.Services.AddScoped<IBusinessUpdateValidationRulesFactory, BusinessUpdateValidationRulesFactory>();
            builder.Services.AddScoped<IBusinessStopValidationRulesFactory, BusinessStopValidationRulesFactory>();
            builder.Services.AddScoped<IBusinessValidationRulesFactory, BusinessValidationRulesFactory>();
            builder.Services.AddScoped<IInputValidationRulesFactory, InputValidationRulesFactory>();
            builder.Services.AddScoped<IRulesConfigurationRepository, RulesConfigurationRepository>();
            builder.Services.AddScoped<IChargeCommandInputValidator, ChargeCommandInputValidator>();
            builder.Services.AddScoped<IChargeCommandBusinessValidator, ChargeCommandBusinessValidator>();
            builder.Services.AddScoped<IChargeCommandValidator, ChargeCommandValidator>();
        }

        private static string GetEnv(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName) ??
                   throw new Exception($"Function app is missing required environment variable '{variableName}'");
        }
    }
}
