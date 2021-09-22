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
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.MeteringPoints.Handlers;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Charges.MeteringPointCreatedReceiver;
using GreenEnergyHub.Messaging.Protobuf;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GreenEnergyHub.Charges.MeteringPointCreatedReceiver
{
    public sealed class Startup : FunctionsStartup
    {
        public override void Configure([NotNull] IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<ICorrelationContext, CorrelationContext>();
            var connectionString = Environment.GetEnvironmentVariable("CHARGE_DB_CONNECTION_STRING") ??
                                   throw new ArgumentNullException(
                                       "CHARGE_DB_CONNECTION_STRING",
                                       "does not exist in configuration settings");
            builder.Services.AddDbContext<ChargesDatabaseContext>(
                options => options.UseSqlServer(connectionString));
            builder.Services.AddScoped<IChargesDatabaseContext, ChargesDatabaseContext>();
            builder.Services.AddScoped<IMeteringPointRepository, MeteringPointRepository>();
            builder.Services.AddScoped<IMeteringPointCreatedEventHandler, MeteringPointCreatedEventHandler>();

            ConfigureMessaging(builder);
        }

        private static void ConfigureMessaging([NotNull] IFunctionsHostBuilder builder)
        {
            builder.Services.ReceiveProtobuf<MeteringPointCreated>(
                configuration => configuration.WithParser(() => MeteringPointCreated.Parser));
        }
    }
}
