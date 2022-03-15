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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;

namespace GreenEnergyHub.Charges.Domain.Charges
{
    public class ChargePeriodFactory : IChargePeriodFactory
    {
        public ChargePeriod CreateUpdateFromChargeOperationDto(ChargeOperationDto chargeOperationDto)
        {
            return Create(chargeOperationDto, false);
        }

        public ChargePeriod CreateStopFromChargeOperationDto(ChargeOperationDto chargeOperationDto)
        {
            return Create(chargeOperationDto, true);
        }

        private static ChargePeriod Create(ChargeOperationDto chargeOperationDto, bool isStop)
        {
            return new ChargePeriod(
                Guid.NewGuid(),
                chargeOperationDto.ChargeName,
                chargeOperationDto.ChargeDescription,
                chargeOperationDto.VatClassification,
                chargeOperationDto.TransparentInvoicing,
                chargeOperationDto.StartDateTime,
                isStop);
        }
    }
}
