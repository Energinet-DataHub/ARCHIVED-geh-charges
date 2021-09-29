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
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Application.MeteringPoints.Handlers;
using GreenEnergyHub.Charges.Core.Currency;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation.BusinessValidation.Factories;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation.InputValidation;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.ChargeLinkCreatedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.DefaultChargeLinks;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Context.Mapping;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeConfirmation;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeLinkCreated;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeRejection;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization.Commands;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;

namespace GreenEnergyHub.Charges.FunctionHost
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(ConfigureServices)
                .Build();

            host.Run();
        }

        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection)
        {
            ConfigureSharedServices(serviceCollection);

            ConfigureChargeIngestion(serviceCollection);
            ConfigureChargeCommandReceiver(serviceCollection);
            ConfigureChargeConfirmationSender(serviceCollection);
            ConfigureChargeRejectionSender(serviceCollection);
            ConfigureChargeLinkIngestion(serviceCollection);
            ConfigureChargeLinkCommandReceiver(serviceCollection);
            ConfigureChargeLinkEventPublisher(serviceCollection);
            ConfigureMeteringPointCreatedReceiver(serviceCollection);
            ConfigureChargeCommandAcceptedReceiver(serviceCollection);
            ConfigureCreateChargeLinkReceiver(serviceCollection);
        }

        private static void ConfigureSharedServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped(typeof(IClock), _ => SystemClock.Instance);
            serviceCollection.AddLogging();

            ConfigureSharedDatabase(serviceCollection);
            ConfigureSharedMessaging(serviceCollection);
        }

        private static void ConfigureSharedDatabase(IServiceCollection serviceCollection)
        {
            var connectionString = Environment.GetEnvironmentVariable("CHARGE_DB_CONNECTION_STRING") ??
                                   throw new ArgumentNullException(
                                       "CHARGE_DB_CONNECTION_STRING",
                                       "does not exist in configuration settings");
            serviceCollection.AddDbContext<ChargesDatabaseContext>(
                options => options.UseSqlServer(connectionString, options => options.UseNodaTime()));
            serviceCollection.AddScoped<IChargesDatabaseContext, ChargesDatabaseContext>();

            serviceCollection.AddScoped<IChargeRepository, ChargeRepository>();
            serviceCollection.AddScoped<IMeteringPointRepository, MeteringPointRepository>();
        }

        private static void ConfigureSharedMessaging(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<MessageDispatcher>();
            serviceCollection.ConfigureProtobufReception();

            serviceCollection.SendProtobuf<ChargeLinkCommandReceivedContract>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinkCommandReceivedEvent>(
                GetEnv("DOMAINEVENT_SENDER_CONNECTION_STRING"),
                GetEnv("CHARGE_LINK_RECEIVED_TOPIC_NAME"));
        }

        private static void ConfigureChargeIngestion(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargesMessageHandler, ChargesMessageHandler>();
            serviceCollection.AddScoped<IChargeCommandHandler, ChargeCommandHandler>();
            serviceCollection
                .AddMessaging()
                .AddMessageExtractor<ChargeCommand>();
            serviceCollection.SendProtobuf<ChargeCommandReceivedContract>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeCommandReceivedEvent>(
                    GetEnv("DOMAINEVENT_SENDER_CONNECTION_STRING"),
                    GetEnv("COMMAND_RECEIVED_TOPIC_NAME"));
        }

        private static void ConfigureChargeCommandReceiver(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargeCommandConfirmationService, ChargeCommandConfirmationService>();
            serviceCollection.AddScoped<IChargeCommandReceivedEventHandler, ChargeCommandReceivedEventHandler>();
            serviceCollection.AddScoped<IChargeFactory, ChargeFactory>();
            serviceCollection.AddScoped<IChargeCommandFactory, ChargeCommandFactory>();
            serviceCollection.AddScoped<IChargeCommandAcceptedEventFactory, ChargeCommandAcceptedEventFactory>();
            serviceCollection.AddScoped<IChargeCommandRejectedEventFactory, ChargeCommandRejectedEventFactory>();

            // DB
            serviceCollection.AddScoped<IMarketParticipantRepository, MarketParticipantRepository>();
            serviceCollection.AddScoped<IMarketParticipantMapper, MarketParticipantMapper>();

            // Validation
            serviceCollection.AddScoped<IBusinessValidationRulesFactory, BusinessValidationRulesFactory>();
            serviceCollection.AddScoped<IInputValidationRulesFactory, InputValidationRulesFactory>();
            serviceCollection.AddScoped<IRulesConfigurationRepository, RulesConfigurationRepository>();
            serviceCollection.AddScoped<IChargeCommandInputValidator, ChargeCommandInputValidator>();
            serviceCollection.AddScoped<IChargeCommandBusinessValidator, ChargeCommandBusinessValidator>();
            serviceCollection.AddScoped<IChargeCommandValidator, ChargeCommandValidator>();

            // ISO 8601 (Timezones)
            const string timeZoneIdString = "LOCAL_TIMEZONENAME";
            var timeZoneId = Environment.GetEnvironmentVariable(timeZoneIdString) ??
                             throw new ArgumentNullException(
                                 timeZoneIdString,
                                 "does not exist in configuration settings");
            var timeZoneConfiguration = new Iso8601ConversionConfiguration(timeZoneId);
            serviceCollection.AddSingleton<IIso8601ConversionConfiguration>(timeZoneConfiguration);
            serviceCollection.AddScoped<IZonedDateTimeService, ZonedDateTimeService>();

            // ISO 4217 (Currencies)
            const string currencyString = "CURRENCY";
            var currency = Environment.GetEnvironmentVariable(currencyString) ??
                             throw new ArgumentNullException(
                                 currencyString,
                                 "does not exist in configuration settings");
            var iso4217Currency = new Iso4217CurrencyConfiguration(currency);
            serviceCollection.AddSingleton(iso4217Currency);

            // Messaging
            serviceCollection.ReceiveProtobufMessage<ChargeCommandReceivedContract>(
                configuration => configuration.WithParser(() => ChargeCommandReceivedContract.Parser));

            serviceCollection.SendProtobuf<ChargeCommandAcceptedContract>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeCommandAcceptedEvent>(
                GetEnv("DOMAINEVENT_SENDER_CONNECTION_STRING"),
                GetEnv("COMMAND_ACCEPTED_TOPIC_NAME"));

            serviceCollection.SendProtobuf<ChargeCommandRejectedContract>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeCommandRejectedEvent>(
                GetEnv("DOMAINEVENT_SENDER_CONNECTION_STRING"),
                GetEnv("COMMAND_REJECTED_TOPIC_NAME"));
        }

        private static void ConfigureChargeConfirmationSender(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargeConfirmationSender, ChargeConfirmationSender>();

            serviceCollection.ReceiveProtobufMessage<ChargeCommandAcceptedContract>(
                configuration => configuration.WithParser(() => ChargeCommandAcceptedContract.Parser));
            serviceCollection.SendProtobuf<ChargeConfirmationContract>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeConfirmation>(
                GetEnv("DOMAINEVENT_SENDER_CONNECTION_STRING"),
                GetEnv("POST_OFFICE_TOPIC_NAME"));
        }

        private static void ConfigureChargeRejectionSender(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargeRejectionSender, ChargeRejectionSender>();

            serviceCollection.ReceiveProtobufMessage<ChargeCommandRejectedContract>(
                configuration => configuration.WithParser(() => ChargeCommandRejectedContract.Parser));
            serviceCollection.SendProtobuf<ChargeRejectionContract>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeRejection>(
                GetEnv("DOMAINEVENT_SENDER_CONNECTION_STRING"),
                GetEnv("POST_OFFICE_TOPIC_NAME"));
        }

        private static void ConfigureChargeLinkIngestion(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ChargeLinkCommandConverter>();
            serviceCollection.AddScoped<MessageExtractor<ChargeLinkCommand>>();
            serviceCollection.AddScoped<MessageDeserializer<ChargeLinkCommand>, ChargeLinkCommandDeserializer>();

            serviceCollection.AddScoped<IChargeLinkCommandHandler, ChargeLinkCommandHandler>();
        }

        private static void ConfigureChargeLinkCommandReceiver(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargeLinkCommandReceivedHandler, ChargeLinkCommandReceivedHandler>();
            serviceCollection.AddScoped<IChargeLinkFactory, ChargeLinkFactory>();
            serviceCollection.AddSingleton<IChargeLinkCommandAcceptedEventFactory, ChargeLinkCommandAcceptedEventFactory>();

            serviceCollection.ReceiveProtobufMessage<ChargeLinkCommandReceivedContract>(
                configuration => configuration.WithParser(() => ChargeLinkCommandReceivedContract.Parser));
            serviceCollection.SendProtobuf<ChargeLinkCommandAcceptedContract>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinkCommandAcceptedEvent>(
                GetEnv("DOMAINEVENT_SENDER_CONNECTION_STRING"),
                GetEnv("CHARGE_LINK_ACCEPTED_TOPIC_NAME"));

            serviceCollection.AddScoped<IChargeLinkRepository, ChargeLinkRepository>();
        }

        private static void ConfigureChargeLinkEventPublisher(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargeLinkCreatedEventFactory, ChargeLinkCreatedEventFactory>();
            serviceCollection.AddScoped<IChargeLinkEventPublishHandler, ChargeLinkEventPublishHandler>();

            serviceCollection.ReceiveProtobufMessage<ChargeLinkCommandAcceptedContract>(
                configuration => configuration.WithParser(() => ChargeLinkCommandAcceptedContract.Parser));

            serviceCollection.SendProtobuf<ChargeLinkCreatedContract>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinkCreatedEvent>(
                GetEnv("INTEGRATIONEVENT_SENDER_CONNECTION_STRING"),
                GetEnv("CHARGE_LINK_CREATED_TOPIC_NAME"));
        }

        private static void ConfigureCreateChargeLinkReceiver(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ICreateLinkCommandEventHandler, CreateLinkCommandEventHandler>();
            serviceCollection.AddScoped<IChargeLinkCommandFactory, ChargeLinkCommandFactory>();

            serviceCollection.ReceiveProtobufMessage<CreateLinkCommandContract>(
                configuration => configuration.WithParser(() => CreateLinkCommandContract.Parser));

            serviceCollection.AddScoped<IDefaultChargeLinkRepository, DefaultChargeLinkRepository>();
        }

        private static void ConfigureMeteringPointCreatedReceiver(IServiceCollection serviceCollection)
        {
            serviceCollection.ReceiveProtobufMessage<MeteringPointCreated>(
                configuration => configuration.WithParser(() => MeteringPointCreated.Parser));

            serviceCollection.AddScoped<IMeteringPointCreatedEventHandler, MeteringPointCreatedEventHandler>();
        }

        private static void ConfigureChargeCommandAcceptedReceiver(IServiceCollection serviceCollection)
        {
            serviceCollection.ReceiveProtobufMessage<ChargeCommandAcceptedContract>(
                configuration => configuration.WithParser(() => ChargeCommandAcceptedContract.Parser));

            serviceCollection.AddScoped<MessageExtractor<ChargeCommandAcceptedEvent>>();
            serviceCollection.AddScoped<IChargeCreatedFactory, ChargeCreatedFactory>();
            serviceCollection.AddScoped<IChargePricesUpdatedFactory, ChargePricesUpdatedFactory>();
            serviceCollection.AddScoped<IChargeSender, ChargeSender>();
            serviceCollection.AddScoped<IChargePricesUpdatedSender, ChargePricesUpdatedSender>();
            serviceCollection.AddScoped<IChargeCommandAcceptedEventHandler, ChargeCommandAcceptedEventHandler>();

            serviceCollection.SendProtobuf<Infrastructure.Integration.ChargeCreated.ChargeCreated>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<GreenEnergyHub.Charges.Domain.Charges.Acknowledgements.ChargeCreated>(
                GetEnv("INTEGRATIONEVENT_SENDER_CONNECTION_STRING"),
                GetEnv("CHARGE_CREATED_TOPIC_NAME"));

            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<GreenEnergyHub.Charges.Domain.Charges.Acknowledgements.ChargePricesUpdated>(
                GetEnv("INTEGRATIONEVENT_SENDER_CONNECTION_STRING"),
                GetEnv("CHARGE_PRICES_UPDATED_TOPIC_NAME"));
        }

        private static string GetEnv(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName) ??
                   throw new Exception($"Function app is missing required environment variable '{variableName}'");
        }
    }
}
