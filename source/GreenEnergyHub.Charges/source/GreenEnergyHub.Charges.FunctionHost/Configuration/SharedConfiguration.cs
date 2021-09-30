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
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class SharedConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
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
                EnvironmentHelper.GetEnv("DOMAINEVENT_SENDER_CONNECTION_STRING"),
                EnvironmentHelper.GetEnv("CHARGE_LINK_RECEIVED_TOPIC_NAME"));
        }
    }
}
