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

using System.Threading.Tasks;
using Energinet.DataHub.Core.Messaging.Transport.SchemaValidation;
using Energinet.DataHub.Core.Schemas;
using Energinet.DataHub.Core.SchemaValidation;

using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;

namespace GreenEnergyHub.Charges.Infrastructure.CimDeserialization.ChargeBundle
{
    public sealed class ChargeCommandDeserializer : SchemaValidatingMessageDeserializer<Domain.Dtos.ChargeInformationCommands.ChargeBundleDto>
    {
        private readonly IChargeCommandConverter _chargeCommandConverter;

        public ChargeCommandDeserializer(IChargeCommandConverter chargeCommandConverter)
            : base(Schemas.CimXml.StructureRequestChangeOfPriceList)
        {
            _chargeCommandConverter = chargeCommandConverter;
        }

        protected override async Task<Domain.Dtos.ChargeInformationCommands.ChargeBundleDto> ConvertAsync(SchemaValidatingReader reader)
        {
            var command = await _chargeCommandConverter
                .ConvertAsync(reader)
                .ConfigureAwait(false);

            return (Domain.Dtos.ChargeInformationCommands.ChargeBundleDto)command;
        }
    }
}
