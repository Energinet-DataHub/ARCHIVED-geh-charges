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
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Application.Validation.InputValidation;
using GreenEnergyHub.Charges.ChargeCommandReceiver;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Charges.Infrastructure.Topics;
using GreenEnergyHub.Json;
using GreenEnergyHub.Messaging;
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
            var connectionString = Environment.GetEnvironmentVariable("CHARGE_DB_CONNECTION_STRING") ??
                                   throw new ArgumentNullException("CHARGE_DB_CONNECTION_STRING", "does not exist in configuration settings");
            builder.Services.AddDbContext<ChargesDatabaseContext>(
                options => options.UseSqlServer(connectionString));
            builder.Services.AddScoped<IChargesDatabaseContext, ChargesDatabaseContext>();
            builder.Services.AddGreenEnergyHub(typeof(ChangeOfChargesMessageHandler).Assembly);
            builder.Services.AddSingleton<IJsonSerializer, JsonSerializer>();
            builder.Services.AddScoped(typeof(IClock), _ => SystemClock.Instance);
            builder.Services.AddScoped<IChangeOfChargeTransactionInputValidator, ChangeOfChargeTransactionInputValidator>();
            builder.Services.AddScoped<IChangeOfChargesTransactionHandler, ChangeOfChargesTransactionHandler>();
            builder.Services
                .AddScoped<IInternalEventCommunicationConfiguration, InternalEventCommunicationConfiguration>();
            builder.Services.AddScoped<IInternalEventPublisher, InternalEventPublisher>();
            builder.Services.AddScoped<IChargeCommandAcknowledgementService, ChargeCommandAcknowledgementService>();
            builder.Services.AddScoped<IChargeCommandHandler, ChargeCommandHandler>();
            builder.Services.AddScoped<IChargeFactory, ChargeFactory>();
            builder.Services.AddScoped<IBusinessValidationRulesFactory, BusinessValidationRulesFactory>();
            builder.Services.AddScoped<IChargeCommandBusinessValidator, ChargeCommandBusinessValidator>();
            builder.Services.AddScoped<IChargeCommandValidator, ChargeCommandValidator>();
            builder.Services.AddScoped<IUpdateRulesConfigurationRepository, UpdateRulesConfigurationRepository>();
            builder.Services.AddScoped<IBusinessUpdateValidationRulesFactory, BusinessUpdateValidationRulesFactory>();
            builder.Services.AddScoped<IChargeRepository, ChargeRepository>();
            builder.Services.AddScoped<IBusinessAdditionValidationRulesFactory, BusinessAdditionValidationRulesFactory>();
            builder.Services.AddScoped<IChargeCommandAcceptedEventFactory, ChargeCommandAcceptedEventFactory>();
        }
    }
}
