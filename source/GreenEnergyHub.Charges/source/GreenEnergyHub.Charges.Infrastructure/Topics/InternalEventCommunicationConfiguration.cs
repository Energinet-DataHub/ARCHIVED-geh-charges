// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.Events.Local;

namespace GreenEnergyHub.Charges.Infrastructure.Topics
{
    public class InternalEventCommunicationConfiguration : IInternalEventCommunicationConfiguration
    {
        private const string TopicConfigurationPostfix = "_TOPIC_NAME";
        private const string ConnectionStringConfigurationPostfix = "_SENDER_CONNECTION_STRING";
        private const string PrefixCommandReceived = "COMMAND_RECEIVED";
        private const string PrefixCommandAccepted = "COMMAND_ACCEPTED";
        private const string PrefixCommandRejected = "COMMAND_REJECTED";

        public string GetTopic(IInternalEvent internalEvent)
        {
            return GetConfiguration(internalEvent, TopicConfigurationPostfix);
        }

        public string GetConnectionString(IInternalEvent internalEvent)
        {
            return GetConfiguration(internalEvent, ConnectionStringConfigurationPostfix);
        }

        private static string GetEnvironmentVariable(string name)
        {
            var environmentVariable = Environment.GetEnvironmentVariable(name) ??
                                      throw new ArgumentNullException(name, "does not exist in configuration settings");
            return environmentVariable;
        }

        private static string GetEventPrefix(IInternalEvent internalEvent)
        {
            return internalEvent switch
            {
                ChargeCommandReceivedEvent => PrefixCommandReceived,
                ChargeCommandAcceptedEvent => PrefixCommandAccepted,
                ChargeCommandRejectedEvent => PrefixCommandRejected,
                _ => throw new ArgumentOutOfRangeException(
                    internalEvent.GetType().ToString(),
                    "is not supported in configuration settings")
            };
        }

        private static string GetConfiguration(IInternalEvent internalEvent, string configurationPostfix)
        {
            var eventPrefix = GetEventPrefix(internalEvent);
            var environmentVariable = $"{eventPrefix}{configurationPostfix}";

            return GetEnvironmentVariable(environmentVariable);
        }
    }
}
