﻿// Copyright 2020 Energinet DataHub A/S
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

using System;

namespace Energinet.DataHub.Charges.Clients
{
    /// <summary>
    /// This static class contains all relative uris / endpoints for the Charges Web API
    /// </summary>
    public static class ChargesRelativeUris
    {
        /// <summary>
        /// Provides the relative uri for getting charge links for a given metering point id.
        /// </summary>
        /// <param name="meteringPointId">The 18-digits metering point identifier used by the Danish version of Green Energy Hub.</param>
        /// <returns>Relative URI including parameter</returns>
        public static Uri GetChargeLinksByMeteringPointId(string meteringPointId)
        {
            return new Uri($"ChargeLinks/GetChargeLinksByMeteringPointIdAsync/?meteringPointId={meteringPointId}", UriKind.Relative);
        }
    }
}
