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

using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.Charges.SystemTests.Fixtures
{
    /// <summary>
    /// Responsible for retrieving settings necessary for performing system tests of business processes in charges domain.
    ///
    /// On developer machines we use the 'systemtest.local.settings.json' to set values.
    /// On hosted agents we must set these using environment variables.
    /// </summary>
    public class BusinessProcessConfiguration : ApiManagementConfiguration
    {
        public BusinessProcessConfiguration()
        {
            ChargeIngestionEndpoint = "/v1.0/cim/requestchangepricelist";
            PeekEndpoint = "/v1.0/cim/masterdata?bundleId=";
            DequeueEndpoint = "/v1.0/cim/dequeue/";
            GridAccessProvider = Root.GetValue<string>("GRID_ACCESS_PROVIDER");
        }

        /// <summary>
        /// Endpoint for submitting charges
        /// </summary>
        public string ChargeIngestionEndpoint { get; }

        /// <summary>
        /// Endpoint for peeking messages from MessageHub
        /// </summary>
        public string PeekEndpoint { get; }

        /// <summary>
        /// Endpoint for dequeuing messages from MessageHub
        /// </summary>
        public string DequeueEndpoint { get; }

        /// <summary>
        /// Grid access provider for submitted charges
        /// </summary>
        public string GridAccessProvider { get; }
    }
}
