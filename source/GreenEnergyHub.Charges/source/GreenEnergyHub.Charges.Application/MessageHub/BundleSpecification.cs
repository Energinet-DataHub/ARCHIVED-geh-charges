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

using GreenEnergyHub.Charges.Domain.AvailableData;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Application.MessageHub
{
    public abstract class BundleSpecification<TAvailableData>
        where TAvailableData : AvailableDataBase
    {
        private readonly BundleType _bundleType;

        protected BundleSpecification(BundleType bundleType)
        {
            _bundleType = bundleType;
        }

        public abstract int GetMessageWeight(TAvailableData data);

        protected virtual string GetMessageType(BusinessReasonCode businessReasonCode)
        {
            // TODO We are right now tied to the naming of our enum, undo this tie in a mapper
            return _bundleType + "_" + businessReasonCode;
        }
    }
}
