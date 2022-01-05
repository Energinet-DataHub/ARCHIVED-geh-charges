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

using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData
{
    /// <summary>
    /// Meta data for messages sent and received by the components of the domain.
    /// </summary>
    public interface IMessageMetaDataContext
    {
        /// <summary>
        /// The moment when the origin request was received (probably by another domain in the system).
        /// </summary>
        Instant RequestDataTime { get; }

        /// <summary>
        /// Returns the ReplyTo value from the Message's Metadata, if no value is present a exception is thrown.
        /// </summary>
        string ReplyTo { get; }

        /// <summary>
        /// Returns true if ReplyTo is set to value that is not empty.
        /// </summary>
        bool IsReplyToSet();

        /// <summary>
        /// Returns the SessionId value from the Message's Metadata, if no value is present a exception is thrown.
        /// </summary>
        string SessionId { get; }
    }
}
