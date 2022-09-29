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
using Energinet.Charges.Contracts.Charge;

namespace GreenEnergyHub.Charges.WebApi.Mappers;

public static class ValidityOptionMapper
{
    public static ValidityOptions Map(string value)
    {
        var isParsed = Enum.TryParse<ValidityOptions>(value, out var validityOption);
        if (!isParsed)
            throw new InvalidEnumArgumentException($"Could not parse {value} as a ValidityOption");

        return validityOption;
    }
}
