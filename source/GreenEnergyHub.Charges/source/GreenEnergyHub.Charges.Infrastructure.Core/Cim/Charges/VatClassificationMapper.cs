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

using System.ComponentModel;
using GreenEnergyHub.Charges.Domain.ChargeInformations;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges
{
    public static class VatClassificationMapper
    {
        private const string CimNoVat = "D01";
        private const string CimVat25 = "D02";

        public static VatClassification Map(string value)
        {
            return value switch
            {
                CimNoVat => VatClassification.NoVat,
                CimVat25 => VatClassification.Vat25,
                _ => VatClassification.Unknown,
            };
        }

        public static string Map(VatClassification vatClassification)
        {
            return vatClassification switch
            {
                VatClassification.NoVat => CimNoVat,
                VatClassification.Vat25 => CimVat25,
                _ => throw new InvalidEnumArgumentException(
                    $"Provided VatClassification value '{vatClassification}' is invalid and cannot be mapped."),
            };
        }
    }
}
