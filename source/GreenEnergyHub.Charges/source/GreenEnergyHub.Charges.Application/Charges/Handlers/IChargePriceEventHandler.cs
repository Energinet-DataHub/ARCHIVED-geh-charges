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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
<<<<<<< HEAD:source/GreenEnergyHub.Charges/source/GreenEnergyHub.Charges.Application/Charges/Handlers/IChargeInformationEventHandler.cs
    public interface IChargeInformationEventHandler
    {
=======
    /// <summary>
    /// Handles events for charges with BusinessReasonCode D08
    /// </summary>
    public interface IChargePriceEventHandler
    {
        /// <summary>
        /// Handles the received event as a chargeprice event
        /// </summary>
        /// <param name="commandReceivedEvent"></param>
>>>>>>> origin/main:source/GreenEnergyHub.Charges/source/GreenEnergyHub.Charges.Application/Charges/Handlers/IChargePriceEventHandler.cs
        Task HandleAsync(ChargeCommandReceivedEvent commandReceivedEvent);
    }
}
