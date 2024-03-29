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
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.Charges
{
    public class ChargeInformationMessagePersisterEndpoint
    {
        private const string FunctionName = nameof(ChargeInformationMessagePersisterEndpoint);
        private readonly JsonMessageDeserializer _deserializer;
        private readonly IChargeInformationMessagePersister _chargeInformationMessagePersister;
        private readonly IUnitOfWork _unitOfWork;

        public ChargeInformationMessagePersisterEndpoint(
            JsonMessageDeserializer deserializer,
            IChargeInformationMessagePersister chargeInformationMessagePersister,
            IUnitOfWork unitOfWork)
        {
            _deserializer = deserializer;
            _chargeInformationMessagePersister = chargeInformationMessagePersister;
            _unitOfWork = unitOfWork;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.ChargesDomainEventTopicName + "%",
                "%" + EnvironmentSettingNames.ChargeInformationOperationsAcceptedPersistMessageSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DataHubListenerConnectionString)]
            byte[] message)
        {
            var chargeCommandAcceptedEvent = await _deserializer
                .FromBytesAsync<ChargeInformationOperationsAcceptedEvent>(message).ConfigureAwait(false);

            await _chargeInformationMessagePersister.PersistMessageAsync(chargeCommandAcceptedEvent).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
