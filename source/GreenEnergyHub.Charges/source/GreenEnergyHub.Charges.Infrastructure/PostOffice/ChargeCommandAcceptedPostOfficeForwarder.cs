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
using GreenEnergyHub.Charges.Application.PostOffice;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Domain.PostOffice;

namespace GreenEnergyHub.Charges.Infrastructure.PostOffice
{
    public class ChargeCommandAcceptedPostOfficeForwarder : IChargeCommandAcceptedPostOfficeForwarder
    {
        private readonly IPostOfficeService _postOfficeService;

        public ChargeCommandAcceptedPostOfficeForwarder(IPostOfficeService postOfficeService)
        {
            _postOfficeService = postOfficeService;
        }

        public async Task HandleAsync([NotNull]ChargeCommandAcceptedEvent acceptedEvent)
        {
            var chargeCommandAcceptedAcknowledgement = new ChargeCommandAcceptedAcknowledgement(acceptedEvent.CorrelationId);
            await _postOfficeService.SendAsync(chargeCommandAcceptedAcknowledgement).ConfigureAwait(false);
        }
    }
}
