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

using System;

namespace GreenEnergyHub.PostOffice.Communicator.Model
{
    /// <summary>
    /// Specifies which data is available for consumption by a market operator.
    /// When a notification is received, the data is immediately made available for peeking.
    /// </summary>
    /// <param name="Uuid">
    /// A guid uniquely identifying the data. This guid will be passed back
    /// to the sub-domain with the request for data to be generated.
    /// </param>
    /// <param name="Recipient">
    /// A Global Location Number identifying the market operator.
    /// </param>
    /// <param name="MessageType">
    /// A unique case-insensitive identification of the type of data.
    /// Data with matching types can be bundled together.
    /// </param>
    /// <param name="Origin">
    /// An enum indentifying the source domain.<br />
    /// - Market operators can request data from a specific origin (domain).<br />
    /// - When data has to be generated, the request will be sent to the specified origin (domain).
    /// </param>
    /// <param name="SupportsBundling">
    /// Allows bundling this data with other data with an identical <paramref name="MessageType" />.
    /// <paramref name="RelativeWeight" /> has no meaning, if bundling is disabled.
    /// </param>
    /// <param name="RelativeWeight">
    /// The weight of the current data. The weight is used to create bundles, where
    /// <c>∑(RelativeWeight) ≤ MaxWeight</c>. MaxWeight is specified by sub-domain.<br/>
    /// The weight and maximum weight are used to ensure
    /// that the resulting bundle stays within the data size limit.
    /// </param>
    public sealed record DataAvailableNotificationDto(
        Guid Uuid,
        GlobalLocationNumberDto Recipient,
        MessageTypeDto MessageType,
        DomainOrigin Origin,
        bool SupportsBundling,
        int RelativeWeight);
}
