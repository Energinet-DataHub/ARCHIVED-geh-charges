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

using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Core.Messaging.MessageTypes.Common;
using Energinet.DataHub.Core.Messaging.Transport;
using Energinet.DataHub.Core.SchemaValidation.Errors;

namespace GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization
{
    public sealed class SchemaValidatedInboundMessage<TInboundMessage> : IInboundMessage
        where TInboundMessage : IInboundMessage
    {
        public SchemaValidatedInboundMessage(TInboundMessage validatedMessage)
        {
            HasErrors = true;
            ValidatedMessage = validatedMessage;
            SchemaValidationError = default;
            Transaction = validatedMessage.Transaction;
        }

        public SchemaValidatedInboundMessage(ErrorResponse schemaValidationError)
        {
            HasErrors = false;
            ValidatedMessage = default;
            SchemaValidationError = schemaValidationError;
            Transaction = Transaction.NewTransaction();
        }

        public Transaction Transaction { get; set; }

        [MemberNotNullWhen(false, nameof(ValidatedMessage))]
        public bool HasErrors { get; }

        public TInboundMessage? ValidatedMessage { get; }

        public ErrorResponse SchemaValidationError { get; }
    }
}
