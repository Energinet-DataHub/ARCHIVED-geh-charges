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
using GreenEnergyHub.Charges.Domain.ChargeCommandAcceptedEvents;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub
{
    /// <summary>
    /// Contract for notifying the MessageHub that data about a charge that has been created
    /// is available.
    /// This is the RSM-034 CIM XML 'NotifyPriceList'.
    /// </summary>
    public interface IChargeDataAvailableNotifier
    {
        Task NotifyAsync(ChargeCommandAcceptedEvent chargeCommandAcceptedEvent);
    }
}
