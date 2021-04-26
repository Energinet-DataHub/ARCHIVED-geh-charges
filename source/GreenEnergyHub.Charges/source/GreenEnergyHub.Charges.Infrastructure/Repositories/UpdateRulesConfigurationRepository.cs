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
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Rules;

namespace GreenEnergyHub.Charges.Infrastructure.Repositories
{
    public class UpdateRulesConfigurationRepository : IUpdateRulesConfigurationRepository
    {
        public Task<UpdateRulesConfiguration> GetConfigurationAsync()
        {
            // For now we just mimic fetching configuration from elsewhere - probably some kind of persistent storage
            var updateRulesConfiguration = new UpdateRulesConfiguration(
                new StartDateVr209ValidationRuleConfiguration(new Domain.Interval<int>(31, 1095)));
            return Task.FromResult(updateRulesConfiguration);
        }
    }
}
