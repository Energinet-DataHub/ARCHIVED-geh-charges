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
using GreenEnergyHub.Charges.ChargeAcknowledgementSender;
using GreenEnergyHub.Charges.Domain;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GreenEnergyHub.Charges.ChargeAcknowledgementSender
{
    public class Startup : FunctionsStartup
    {
        public override void Configure([NotNull] IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped(typeof(IClock), _ => SystemClock.Instance);
            builder.Services.AddScoped<IChargeAcknowledgementSender, Infrastructure.ChargeAcknowledgementSender>();

            builder.Services
                .AddMessaging()
                .AddMessageDispatcher<ChargeAcknowledgement>(
                    GetEnv("POST_OFFICE_SENDER_CONNECTION_STRING"),
                    GetEnv("POST_OFFICE_TOPIC_NAME"))
                .AddMessageExtractor<ChargeCommandAcceptedEvent>();
        }

        private static string GetEnv(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName) ??
                   throw new Exception($"Function app is missing required environment variable '{variableName}'");
        }
    }
}
