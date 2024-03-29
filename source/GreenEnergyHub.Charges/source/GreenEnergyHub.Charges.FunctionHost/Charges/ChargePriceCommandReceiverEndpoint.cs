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
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargePrice;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.Charges
{
    public class ChargePriceCommandReceiverEndpoint
    {
        private const string FunctionName = nameof(ChargePriceCommandReceiverEndpoint);
        private readonly IChargePriceCommandReceivedEventHandler _chargePriceCommandReceivedEventHandler;
        private readonly JsonMessageDeserializer _deserializer;
        private readonly IUnitOfWork _unitOfWork;

        public ChargePriceCommandReceiverEndpoint(
            IChargePriceCommandReceivedEventHandler chargePriceCommandReceivedEventHandler,
            JsonMessageDeserializer deserializer,
            IUnitOfWork unitOfWork)
        {
            _chargePriceCommandReceivedEventHandler = chargePriceCommandReceivedEventHandler;
            _deserializer = deserializer;
            _unitOfWork = unitOfWork;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.ChargesDomainEventTopicName + "%",
                "%" + EnvironmentSettingNames.ChargePriceCommandReceivedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DataHubListenerConnectionString)]
            byte[] message)
        {
            var chargePriceCommandReceivedEvent = await _deserializer
                .FromBytesAsync<ChargePriceCommandReceivedEvent>(message)
                .ConfigureAwait(false);

            await _chargePriceCommandReceivedEventHandler
                .HandleAsync(chargePriceCommandReceivedEvent)
                .ConfigureAwait(false);

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
