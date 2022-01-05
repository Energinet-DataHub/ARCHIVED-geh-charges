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

using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Application.MessageHub
{
    public abstract class BundleSpecification<TAvailableData, TInputType> : IBundleSpecification<TAvailableData>
        where TAvailableData : AvailableDataBase
    {
        private readonly BundleType _bundleType;

        protected BundleSpecification(BundleType bundleType)
        {
            _bundleType = bundleType;
        }

        public abstract int GetMessageWeight(TAvailableData data);

        public string GetMessageType(BusinessReasonCode businessReasonCode)
        {
            // NOTE: While we need to map the bundle type to make sure refactorings of the enum does not
            // directly impact the interface between the charge domain and the messagehub, the business
            // reason code is less dangerous as it just needs to be the same on stuff that can be in the
            // same bundle. Worst case, a refactoring just means you get your information in two messages
            // instead of one.
            return BundleTypeMapper.Map(_bundleType) + "_" + businessReasonCode;
        }
    }
}
