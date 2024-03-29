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

using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.Charges.MessageHub
{
    /// <summary>
    /// The function will initiate the communication with MessageHub
    /// by notifying that charge price data is available
    /// This is the RSM-034 CIM XML 'NotifyPriceList'.
    /// </summary>
    public class ChargePriceDataAvailableNotifierEndpoint
    {
        private const string FunctionName = nameof(ChargePriceDataAvailableNotifierEndpoint);
        private readonly JsonMessageDeserializer _deserializer;
        private readonly IAvailableDataNotifier<AvailableChargePriceData, ChargePriceOperationsAcceptedEvent> _availableDataNotifier;

        public ChargePriceDataAvailableNotifierEndpoint(
            JsonMessageDeserializer deserializer,
            IAvailableDataNotifier<AvailableChargePriceData, ChargePriceOperationsAcceptedEvent> availableDataNotifier)
        {
            _deserializer = deserializer;
            _availableDataNotifier = availableDataNotifier;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.ChargesDomainEventTopicName + "%",
                "%" + EnvironmentSettingNames.ChargePriceOperationsAcceptedDataAvailableSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DataHubListenerConnectionString)]
            byte[] message)
        {
            var chargePriceOperationsConfirmedEvent = await _deserializer
                .FromBytesAsync<ChargePriceOperationsAcceptedEvent>(message).ConfigureAwait(false);
            await _availableDataNotifier.NotifyAsync(chargePriceOperationsConfirmedEvent).ConfigureAwait(false);
        }
    }
}
