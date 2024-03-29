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

using System;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.JsonSerialization;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Core.InternalMessaging;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Factories;
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.QueryServices;
using GreenEnergyHub.Charges.WebApi.Factories;
using GreenEnergyHub.Iso8601;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace GreenEnergyHub.Charges.WebApi
{
    public static class QueryApiConfiguration
    {
        public static void AddQueryApi(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddDbContext<QueryDbContext>(
                options =>
                {
                    var connectionString = configuration.GetConnectionString(EnvironmentSettingNames.ChargeDbConnectionString);

                    if (connectionString == null)
                        throw new ArgumentNullException(EnvironmentSettingNames.ChargeDbConnectionString, "does not exist in configuration settings");

                    options.UseSqlServer(connectionString);
                });

            serviceCollection.AddSingleton(_ =>
            {
                var connectionString = configuration.GetValue<string>(EnvironmentSettingNames.DataHubSenderConnectionString);
                return new ServiceBusClient(connectionString);
            });

            // Must be a singleton as per documentation of ServiceBusClient and ServiceBusSender
            serviceCollection.AddSingleton<IServiceBusDispatcher>(
                sp =>
                {
                    var topicName = configuration.GetValue<string>(EnvironmentSettingNames.ChargesDomainEventTopicName);
                    var serviceBusClient = sp.GetRequiredService<ServiceBusClient>();
                    return new ServiceBusDispatcher(serviceBusClient, topicName);
                });

            serviceCollection.AddScoped<IChargeInformationCommandFactory>(
                sp =>
            {
                var meteringPointAdministratorGln = configuration.GetValue<string>(EnvironmentSettingNames.MeteringPointAdministratorGln);
                var clock = sp.GetRequiredService<IClock>();
                return new ChargeInformationCommandFactory(clock, meteringPointAdministratorGln);
            });

            serviceCollection.AddScoped<IData, Data>();
            serviceCollection.AddScoped<IChargesQueryService, ChargesQueryService>();
            serviceCollection.AddScoped<IMarketParticipantQueryService, MarketParticipantQueryService>();
            serviceCollection.AddScoped<IChargePriceQueryService, ChargePriceQueryService>();
            serviceCollection.AddScoped<IChargeMessageQueryService, ChargeMessageQueryService>();
            serviceCollection.AddScoped<IChargeHistoryQueryService, ChargeHistoryQueryService>();
            serviceCollection.AddScoped<IChargeInformationCommandHandler, ChargeInformationCommandHandler>();
            serviceCollection.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            serviceCollection.AddScoped<IMessageMetaDataContext, MessageMetaDataContext>();
            serviceCollection.AddScoped<ICorrelationContext, CorrelationContext>();
            serviceCollection.AddScoped<IServiceBusMessageFactory, ServiceBusMessageFactory>();
            serviceCollection.AddScoped(typeof(IClock), _ => SystemClock.Instance);

            serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();

            ConfigureIso8601Services(serviceCollection, configuration);
        }

        private static void ConfigureIso8601Services(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<IIso8601ConversionConfiguration>(_ =>
            {
                var timeZoneId = configuration.GetValue<string>(EnvironmentSettingNames.LocalTimeZoneName);
                var timeZoneConfiguration = new Iso8601ConversionConfiguration(timeZoneId);
                return timeZoneConfiguration;
            });
            serviceCollection.AddSingleton<IIso8601Durations, Iso8601Durations>();
        }
    }
}
