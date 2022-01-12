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

// ReSharper disable once CheckNamespace
namespace GreenEnergyHub.Charges.Infrastructure.Integration.ChargeConfirmation
{
    /// <summary>
    /// ProtoBuf doesn't support decimal.
    /// This implementation is inspired by https://docs.microsoft.com/en-us/aspnet/core/grpc/protobuf?view=aspnetcore-5.0#decimals.
    ///
    /// <see cref="Units"/>: Whole units part of the amount
    /// <see cref="Nanos"/>: Nano units of the amount (10^-9). Must be same sign as units
    ///
    /// Example: 12345.6789 -> { units = 12345, nanos = 678900000 }
    /// </summary>
    public partial class DecimalValue
    {
        private const decimal NanoFactor = 1_000_000_000;

        private DecimalValue(long units, int nanos)
        {
            Units = units;
            Nanos = nanos;
        }

        public static implicit operator decimal(DecimalValue grpcDecimal)
        {
            return grpcDecimal.Units + (grpcDecimal.Nanos / NanoFactor);
        }

        public static implicit operator DecimalValue(decimal value)
        {
            var units = decimal.ToInt64(value);
            var nanos = decimal.ToInt32((value - units) * NanoFactor);
            return new DecimalValue(units, nanos);
        }

        public static DecimalValue FromDecimal(decimal value)
        {
            return value;
        }

        public decimal ToDecimal()
        {
            return this;
        }
    }
}
