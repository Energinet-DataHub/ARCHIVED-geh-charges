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
    /// Responsible for extracting secrets necessary for performing system tests of API Management.
    ///
    /// On developer machines we use the 'systemtest.local.settings.json' to set values.
    /// On hosted agents we must set these using environment variables.
    ///
    /// Developers, and the service principal under which the tests are executed, must have access to the Key Vault
    /// so secrets can be extracted.
    /// </summary>
    public class BusinessProcessConfiguration : ApiManagementConfiguration
    {
        public BusinessProcessConfiguration()
        {
            ChargeIngestionEndpoint = Root.GetValue<string>("CHARGE_INGESTION_ENDPOINT");
            PeekEndpoint = Root.GetValue<string>("PEEK_ENDPOINT");
            DequeueEndpoint = Root.GetValue<string>("DEQUEUE_ENDPOINT");
            GridAccessProvider = Root.GetValue<string>("GRID_ACCESS_PROVIDER");
            MarketParticipant = Root.GetValue<string>("MARKET_PARTICIPANT");
        }

        /// <summary>
        /// Endpoint for submitting charges
        /// </summary>
        public string ChargeIngestionEndpoint { get; }

        /// <summary>
        /// Endpoint for peeking messages from Post Office
        /// </summary>
        public string PeekEndpoint { get; }

        /// <summary>
        /// Endpoint for dequeuing messages from Post Office
        /// </summary>
        public string DequeueEndpoint { get; }

        /// <summary>
        /// Grid access provider for submitted charges
        /// </summary>
        public string GridAccessProvider { get; }

        /// <summary>
        /// Market participant id for submitted charges
        /// </summary>
        public string MarketParticipant { get; }
    }
}
