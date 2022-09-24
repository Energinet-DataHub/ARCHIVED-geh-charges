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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GreenEnergyHub.Charges.Domain.Charges.Exceptions
{
    public class InvalidOperationExceptionExtension : InvalidOperationException
    {
        /// <summary>Throws an <see cref="InvalidOperationException"/> if <paramref name="enumerable"/> is null or contains no elements.</summary>
        /// <param name="enumerable">The reference type enumerable to validate as non-null and containing elements.</param>
        /// <param name="errorMessage">The error message to provide with the <see cref="InvalidOperationException"/>.</param>
        public static void ThrowIfNoElements([NotNull] IEnumerable<object>? enumerable, string errorMessage)
        {
            if (enumerable is null || !enumerable.Any())
            {
                throw new InvalidOperationException(errorMessage);
            }
        }
    }
}
