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

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim
{
    /// <summary>
    /// All reason (error) codes and error texts origins from an Energinet internal document.
    /// This is according to story https://github.com/Energinet-DataHub/geh-charges/issues/922.
    ///
    /// IMPORTANT: The name of the enum field names are used as error code in CIM documents.
    /// </summary>
    public enum ReasonCode
    {
        D01 = 1,
        D02 = 2,
        D03 = 3,
        D04 = 4,
        D05 = 5,
        D06 = 6,
        D07 = 7,
        D08 = 8,
        D09 = 9,
        D11 = 11,
        D12 = 12,
        D13 = 13,
        D14 = 14,
        D15 = 15,
        D16 = 16,
        D17 = 17,
        D18 = 18,
        D19 = 19,
        D20 = 20,
        D21 = 21,
        D22 = 22,
        D23 = 23,
        D24 = 24,
        D25 = 25,
        D26 = 26,
        D27 = 27,
        D28 = 28,
        D29 = 29,
        D30 = 30,
        D31 = 31,
        D32 = 32,
        D33 = 33,
        D34 = 34,
        D35 = 35,
        D36 = 36,
        D37 = 37,
        D38 = 38,
        D39 = 39,
        D40 = 40,
        D41 = 41,
        D42 = 42,
        D43 = 43,
        D44 = 44,
        D45 = 45,
        D46 = 46,
        D47 = 47,
        D48 = 48,
        D49 = 49,
        D50 = 50,
        D51 = 51,
        D52 = 52,
        D53 = 53,
        D54 = 54,
        D55 = 55,
        D56 = 56,
        D57 = 57,
        D58 = 58,
        D59 = 59,
        D60 = 60,
        D61 = 61,
        D62 = 62,
        D63 = 63,
        D67 = 67,
        E09 = 109,
        E0H = 1000,
        E0I = 1001,
        E10 = 110,
        E11 = 111,
        E14 = 114,
        E16 = 116,
        E17 = 117,
        E18 = 118,
        E19 = 119,
        E22 = 122,
        E29 = 129,
        E47 = 147,
        E50 = 150,
        E51 = 151,
        E55 = 155,
        E59 = 159,
        E61 = 161,
        E73 = 173,
        E81 = 181,
        E86 = 186,
        E87 = 187,
        E90 = 190,
        E91 = 191,
        E97 = 197,
        E98 = 198,
        E99 = 999, // TODO: Find correct reason code
    }
}
