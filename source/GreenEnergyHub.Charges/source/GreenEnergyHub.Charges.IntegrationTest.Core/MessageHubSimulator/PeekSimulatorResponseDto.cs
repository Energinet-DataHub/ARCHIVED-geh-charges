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
using System.Diagnostics.CodeAnalysis;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.MessageHubSimulator
{
    public sealed class PeekSimulatorResponseDto
    {
        internal PeekSimulatorResponseDto()
        {
            IsSuccess = false;
        }

        internal PeekSimulatorResponseDto(Guid correlationId, string referenceId, AzureBlobContentDto? content)
        {
            IsSuccess = true;
            CorrelationId = correlationId;
            DataAvailableNotificationReferenceId = referenceId;
            Content = content;
        }

        /// <summary>
        /// Returns true if the request succeeded.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Content))]
        [MemberNotNullWhen(true, nameof(CorrelationId))]
        [MemberNotNullWhen(true, nameof(DataAvailableNotificationReferenceId))]
        public bool IsSuccess { get; }

        /// <summary>
        /// The MessageHub correlation id for the Peek+Dequeue operation.
        /// </summary>
        public Guid? CorrelationId { get; }

        /// <summary>
        /// Contains information about the contents of the bundle generated during Peek.
        /// </summary>
        public AzureBlobContentDto? Content { get; }

        internal string? DataAvailableNotificationReferenceId { get; }
    }
}
