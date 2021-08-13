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
using GreenEnergyHub.Charges.Domain.Acknowledgements;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeConfirmation;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeRejection;
using GreenEnergyHub.Charges.Infrastructure.Integration.Mappers;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public static class ProtobufDeserializationHelper
    {
        public static T Deserialize<T>(byte[] data)
        {
            if (typeof(T) == typeof(ChargeConfirmation))
            {
                return (T)DeserializeChargeConfirmation(data);
            }

            if (typeof(T) == typeof(ChargeRejection))
            {
                return (T)DeserializeChargeRejection(data);
            }

            throw new NotImplementedException("Missing implementation on how to deserialize " + typeof(T).FullName);
        }

        private static IInboundMessage DeserializeChargeConfirmation(byte[] data)
        {
            var mapper = new ChargeConfirmationInboundMapper();
            var parsed = ChargeConfirmationContract.Parser.ParseFrom(data);
            var mapped = mapper.Convert(parsed);
            return mapped;
        }

        private static IInboundMessage DeserializeChargeRejection(byte[] data)
        {
            var mapper = new ChargeRejectionInboundMapper();
            var parsed = ChargeRejectionContract.Parser.ParseFrom(data);
            var mapped = mapper.Convert(parsed);
            return mapped;
        }
    }
}
