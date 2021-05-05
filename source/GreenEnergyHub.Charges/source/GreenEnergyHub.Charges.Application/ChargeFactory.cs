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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    public class ChargeFactory : IChargeFactory
    {
        public Task<Charge> CreateFromCommandAsync([NotNull]ChargeCommand command)
        {
            // Quick and dirty as long as Charge is a command
            var c = new Charge
            {
                ChargeNew = command.ChargeNew,
                Document = command.Document,
                ChargeEvent = command.ChargeEvent,
            };
            return Task.FromResult(c);
        }
    }
}
