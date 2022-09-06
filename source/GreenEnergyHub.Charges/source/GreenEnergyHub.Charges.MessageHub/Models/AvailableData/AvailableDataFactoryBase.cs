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

using System.Collections.Generic;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableData
{
    public abstract class AvailableDataFactoryBase<TAvailableData, TInput> : IAvailableDataFactory<TAvailableData, TInput>
        where TAvailableData : AvailableDataBase
    {
        private readonly IMarketParticipantRepository _marketParticipantRepository;

        protected AvailableDataFactoryBase(IMarketParticipantRepository marketParticipantRepository)
        {
            _marketParticipantRepository = marketParticipantRepository;
        }

        public abstract Task<IReadOnlyList<TAvailableData>> CreateAsync(TInput input);

        protected async Task<MarketParticipant> GetSenderAsync() =>
            await _marketParticipantRepository.GetMeteringPointAdministratorAsync().ConfigureAwait(false);

        protected async Task<MarketParticipant> GetRecipientAsync(MarketParticipantDto marketParticipantDto) =>
            await _marketParticipantRepository.GetSystemOperatorOrGridAccessProviderAsync(marketParticipantDto
                .MarketParticipantId).ConfigureAwait(false);
    }
}
