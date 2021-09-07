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
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Context.EntityConfigurationExtensions
{
    public static class PropertyBuilderExtensions
    {
        /// <summary>
        /// Configure conversion between SQL type "datetime2" and Noda Time <see cref="Instant"/>.
        /// </summary>
        public static PropertyBuilder<Instant> HasNodaTimeInstantConversion(this PropertyBuilder<Instant> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.HasConversion(
                toDbValue => toDbValue.ToDateTimeUtc(),
                fromDbValue => Instant.FromDateTimeUtc(fromDbValue.ToUniversalTime()));
        }

        /// <summary>
        /// Configure conversion between SQL type "datetime2" and Noda Time <see cref="Nullable{Instant}"/>.
        /// </summary>
        public static PropertyBuilder<Instant?> HasNodaTimeInstantConversion(this PropertyBuilder<Instant?> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.HasConversion(
                toDbValue => toDbValue!.Value.ToDateTimeUtc(),
                fromDbValue => Instant.FromDateTimeUtc(fromDbValue.ToUniversalTime()));
        }
    }
}
