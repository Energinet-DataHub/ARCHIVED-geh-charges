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
        /// <summary>Throws an <see cref="InvalidXmlValueException"/> if <paramref name="elementName"/> is am element that has an invalid value.</summary>
        /// <param name="elementName">The cim element name to validate that has an invalid value</param>
        /// <param name="message">details on the invalid value</param>
        public InvalidXmlValueException(string elementName, string message)
            : base($"{elementName} element contains an invalid value. {message}")
        {
        }

        protected InvalidXmlValueException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
