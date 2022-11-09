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
using System.Runtime.Serialization;

namespace GreenEnergyHub.Charges.Infrastructure.CimDeserialization.MarketDocument
{
    [Serializable]
    public class InvalidXmlValueException : Exception
    {
        /// <summary>Throws an <see cref="InvalidXmlValueException"/> if <paramref name="value"/> is empty or contains only whitespace.</summary>
        /// <param name="value">The cim element name to validate as empty or contains only whitespace.</param>
        public InvalidXmlValueException(string value)
            : base($"{value} element contains an invalid value. It is either empty or contains only whitespace.")
        {
        }

        protected InvalidXmlValueException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
