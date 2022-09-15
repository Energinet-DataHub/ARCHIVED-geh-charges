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

using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using GreenEnergyHub.Charges.MessageHub.BundleSpecification;
using GreenEnergyHub.Charges.MessageHub.BundleSpecification.Charges;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeRejectionDataAvailableNotifierEndpointConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IAvailableDataNotifier<AvailableChargeReceiptData, ChargeInformationCommandRejectedEvent>,
                AvailableDataNotifier<AvailableChargeReceiptData, ChargeInformationCommandRejectedEvent>>();
            serviceCollection.AddScoped<AvailableChargeReceiptValidationErrorFactory,
                AvailableChargeReceiptValidationErrorFactory>();
            serviceCollection.AddScoped<ICimValidationErrorTextProvider, CimValidationErrorTextProvider>();
            serviceCollection.AddScoped<ICimValidationErrorCodeFactory, CimValidationErrorCodeFactory>();
            serviceCollection.AddScoped<ICimValidationErrorTextFactory<ChargeInformationOperationDto>,
                ChargeCimValidationErrorTextFactory>();
            serviceCollection.AddScoped<IAvailableDataFactory<AvailableChargeReceiptData, ChargeInformationCommandRejectedEvent>,
                AvailableChargeRejectionDataFactory>();
            serviceCollection.AddScoped<IAvailableDataNotificationFactory<AvailableChargeReceiptData>,
                AvailableDataNotificationFactory<AvailableChargeReceiptData>>();
            serviceCollection.AddScoped<BundleSpecification<AvailableChargeReceiptData, ChargeInformationCommandRejectedEvent>,
                ChargeRejectionBundleSpecification>();
            serviceCollection.AddScoped<JsonMessageDeserializer<ChargeInformationCommandRejectedEvent>>();
        }
    }
}
