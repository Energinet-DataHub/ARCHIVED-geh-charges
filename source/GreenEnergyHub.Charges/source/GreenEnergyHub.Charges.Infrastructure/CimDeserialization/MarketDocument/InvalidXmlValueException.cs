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
using System.ComponentModel;
using System.Runtime.Serialization;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;

namespace GreenEnergyHub.Charges.Infrastructure.CimDeserialization.MarketDocument
{
    [Serializable]
    public class InvalidXmlValueException : Exception
    {
        /// <summary>Throws an <see cref="InvalidXmlValueException"/> if <paramref name="errorIdentifier"/> is an element that has an invalid value.</summary>
        /// <param name="errorCode">Defines what type of XmlError that triggered the exception</param>
        /// <param name="errorIdentifier">The cim element name to validate that has an invalid value</param>
        /// <param name="errorContent">The invalid value</param>
        public InvalidXmlValueException(B2BErrorCode errorCode, string errorIdentifier, string errorContent)
            : base(CreateErrorMessage(errorCode, errorIdentifier, errorContent))
        {
        }

        private static string CreateErrorMessage(B2BErrorCode errorCode, string errorIdentifier, string errorContent) =>
            errorCode switch
            {
                B2BErrorCode.IsEmptyOrWhitespaceErrorMessage => B2BErrorMessageFactory
                    .CreateIsEmptyOrWhitespaceErrorMessage(errorIdentifier).WriteAsXmlString(),
                B2BErrorCode.CouldNotMapEnumErrorMessage => B2BErrorMessageFactory
                    .CreateCouldNotMapEnumErrorMessage(errorIdentifier, errorContent).WriteAsXmlString(),
                _ => throw new InvalidEnumArgumentException(
                    $"Provided B2BCode value '{errorCode}' is invalid."),
            };

        protected InvalidXmlValueException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
