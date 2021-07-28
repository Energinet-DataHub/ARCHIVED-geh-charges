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
using GreenEnergyHub.Charges.Application.Acknowledgement;
using GreenEnergyHub.Charges.ChargeConfirmationSender;
using GreenEnergyHub.Charges.Domain.Acknowledgements;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeConfirmation;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Messaging.Protobuf;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GreenEnergyHub.Charges.ChargeConfirmationSender
{
    public class Startup : FunctionsStartup
    {
        public override void Configure([NotNull] IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped(typeof(IClock), _ => SystemClock.Instance);
            builder.Services.AddScoped<IChargeConfirmationSender, Application.Acknowledgement.ChargeConfirmationSender>();

            ConfigureMessaging(builder.Services);
        }

        private static void ConfigureMessaging(IServiceCollection services)
        {
            services.ReceiveProtobuf<ChargeLinkCommandAcceptedContract>(
                configuration => configuration.WithParser(() => ChargeLinkCommandAcceptedContract.Parser));
            services.SendProtobuf<ChargeConfirmationContract>();
            services.AddMessagingProtobuf().AddMessageDispatcher<ChargeConfirmation>(
                GetEnv("POST_OFFICE_SENDER_CONNECTION_STRING"),
                GetEnv("POST_OFFICE_TOPIC_NAME"));
        }

        private static string GetEnv(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName) ??
                   throw new Exception($"Function app is missing required environment variable '{variableName}'");
        }
    }
}
