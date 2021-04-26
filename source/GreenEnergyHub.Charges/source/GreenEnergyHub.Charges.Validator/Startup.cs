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
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.LocalMessageServiceBusTopicTrigger;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.Json;
using GreenEnergyHub.Messaging;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GreenEnergyHub.Charges.LocalMessageServiceBusTopicTrigger
{
    public class Startup : FunctionsStartup
    {
        public override void Configure([NotNull] IFunctionsHostBuilder builder)
        {
            builder.Services.AddGreenEnergyHub(typeof(ChangeOfChargesMessageHandler).Assembly);
            builder.Services.AddSingleton<IJsonSerializer, JsonSerializer>();
            builder.Services
                .AddScoped<IChangeOfChargeTransactionInputValidator, ChangeOfChargeTransactionInputValidator>();
            builder.Services.AddScoped<IChangeOfChargesTransactionHandler, ChangeOfChargesTransactionHandler>();
            builder.Services.AddScoped<ILocalEventPublisher, LocalEventPublisher>();

            AddIso8601Services(builder.Services);
        }

        private static void AddIso8601Services(IServiceCollection services)
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
    }
}
