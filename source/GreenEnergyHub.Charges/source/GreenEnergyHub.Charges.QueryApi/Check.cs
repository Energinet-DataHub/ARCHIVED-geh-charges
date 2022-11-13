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
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GreenEnergyHub.Charges.QueryApi
{
    public static class Check
    {
        public static T IsNullOrEmpty<T>(T value, string parameterName)
        {
            NotEmpty(parameterName, parameterName);

            ArgumentNullException.ThrowIfNull(value);

            return value;
        }

        private static string NotEmpty(string value, string parameterName)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (value.Trim().Length == 0)
            {
                throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
            }

            return value;
        }
    }

    /*[DebuggerStepThrough]
    internal static class Check
    {
        [ContractAnnotation("value:null => halt")]
        [return: NotNull]
        public static T NotNull<T>([NoEnumeration] [AllowNull] [NotNull] T value, [InvokerParameterName] string parameterName)
        {
            if (value is null)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static IReadOnlyList<T> NotEmpty<T>(
            [NotNull] IReadOnlyList<T>? value,
            [InvokerParameterName] string parameterName)
        {
            NotNull(value, parameterName);

            if (value.Count == 0)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(AbstractionsStrings.CollectionArgumentIsEmpty(parameterName));
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static string NotEmpty([NotNull] string? value, [InvokerParameterName] string parameterName)
        {
            if (value is null)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }

            if (value.Trim().Length == 0)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
            }

            return value;
        }

        public static string? NullButNotEmpty(string? value, [InvokerParameterName] string parameterName)
        {
            if (value is not null && value.Length == 0)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
            }

            return value;
        }

        public static IReadOnlyList<T> HasNoNulls<T>(
            [NotNull] IReadOnlyList<T>? value,
            [InvokerParameterName] string parameterName)
            where T : class
        {
            NotNull(value, parameterName);

            if (value.Any(e => e == null))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(parameterName);
            }

            return value;
        }

        public static IReadOnlyList<string> HasNoEmptyElements(
            [NotNull] IReadOnlyList<string>? value,
            [InvokerParameterName] string parameterName)
        {
            NotNull(value, parameterName);

            if (value.Any(s => string.IsNullOrWhiteSpace(s)))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(AbstractionsStrings.CollectionArgumentHasEmptyElements(parameterName));
            }

            return value;
        }

        [Conditional("DEBUG")]
        public static void DebugAssert([DoesNotReturnIf(false)] bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"Check.DebugAssert failed: {message}");
            }
        }
    }*/
}
