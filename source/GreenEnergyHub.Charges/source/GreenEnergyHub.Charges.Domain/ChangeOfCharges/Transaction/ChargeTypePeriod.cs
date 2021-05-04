﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Collections.Generic;
#pragma warning disable 8618

namespace GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction
{
    // Non-nullable member is uninitialized is ignored
    // Only properties which is allowed to be null is nullable
    // ChargeTypePeriod integrity is null checked by ChargeCommandNullChecker
    public class ChargeTypePeriod
    {
        public ChargeTypePeriod()
        {
            Points = new ();
        }

        public string Resolution { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227", Justification = "JSON deserialization")]
        public List<Point> Points { get; set; }

        public void AddPoints(IEnumerable<Point> newPoints)
        {
            Points.AddRange(newPoints);
        }
    }
}
