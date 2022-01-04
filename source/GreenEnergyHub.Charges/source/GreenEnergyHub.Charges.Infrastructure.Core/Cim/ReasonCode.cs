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
        [ErrorText("The document is approved")]
        D01 = 1,
        [ErrorText("General error")]
        D02 = 2,
        [ErrorText("Missing consumer name or address")]
        D03 = 3,
        [ErrorText("Not used")]
        D04 = 4,
        [ErrorText("Metering point ID does not match the one from the original document")]
        D05 = 5,
        [ErrorText("Reference to transaction ID does not match the one from the original document")]
        D06 = 6,
        [ErrorText("Ongoing move process")]
        D07 = 7,
        [ErrorText("Balance supplier does not match the current Balance Supplier")]
        D08 = 8,
        [ErrorText("Not used")]
        D09 = 9,
        [ErrorText("Combination of search criteria not possible")]
        D11 = 11,
        [ErrorText("Invalid Quality Code")]
        D12 = 12,
        [ErrorText("DataHub Internal error")]
        D13 = 13,
        [ErrorText("Incorrect charge information")]
        D14 = 14,
        [ErrorText("Incorrect settlement")]
        D15 = 15,
        [ErrorText("Incorrect connection status")]
        D16 = 16,
        [ErrorText("Incorrect CPR/CVR")]
        D17 = 17,
        [ErrorText("Incorrect type of meteringpoint")]
        D18 = 18,
        [ErrorText("Functioncode not allowed")]
        D19 = 19,
        [ErrorText("Violated process")]
        D20 = 20,
        [ErrorText("Cancel Meterreading")]
        D21 = 21,
        [ErrorText("Change of supply on MP, new")]
        D22 = 22,
        [ErrorText("Resolution not correct")]
        D23 = 23,
        [ErrorText("Incorrect contract information")]
        D24 = 24,
        [ErrorText("Balance Responsible Party does not match the current Balance Responsible Party")]
        D25 = 25,
        [ErrorText("Unauthorized TSO")]
        D26 = 26,
        [ErrorText("Illegal request")]
        D27 = 27,
        [ErrorText("Service request rejected")]
        D28 = 28,
        [ErrorText("No existing contract")]
        D29 = 29,
        [ErrorText("The attribute cannot be updated in this process")]
        D30 = 30,
        [ErrorText("Incorrect meter information according to rules")]
        D31 = 31,
        [ErrorText("Metering point sub type cannot be changed")]
        D32 = 32,
        [ErrorText("Metering point is part of a calculation structure")]
        D33 = 33,
        [ErrorText("Parent metering point has children")]
        D34 = 34,
        [ErrorText("Balance supplier exist at metering point")]
        D35 = 35,
        [ErrorText("Metering point cannot be connected")]
        D36 = 36,
        [ErrorText("Illegal metering point sub type")]
        D37 = 37,
        [ErrorText("Stop of supply not registered for metering point")]
        D38 = 38,
        [ErrorText("Ongoing stop of supply")]
        D39 = 39,
        [ErrorText("Illegal process")]
        D40 = 40,
        [ErrorText("The municipality must be involved in the disconnection")]
        D41 = 41,
        [ErrorText("The police must be involved in the disconnection")]
        D42 = 42,
        [ErrorText("The bailiff’s court must be involved in the disconnection")]
        D43 = 43,
        [ErrorText("Other rejection reason")]
        D44 = 44,
        [ErrorText("Rejection 5")]
        D45 = 45,
        [ErrorText("Incorrect MeteringGridArea")]
        D46 = 46,
        [ErrorText("Operation not allowed for net settlement group 6")]
        D47 = 47,
        [ErrorText("Marketplayer is blocked for operation in this MeteringGridArea")]
        D48 = 48,
        [ErrorText("Other marketplayer is blocked for operation in this MeteringGridArea")]
        D49 = 49,
        [ErrorText("No delegation found")]
        D50 = 50,
        [ErrorText("Change of electrical heating status not allowed")]
        D51 = 51,
        [ErrorText("Process could not be carried out. Please contact DataHub Support")]
        D52 = 52,
        [ErrorText("Incorrect MeterReading Occurrence")]
        D53 = 53,
        [ErrorText("A move is not allowed because of a completed “End of Supply” process")]
        D54 = 54,
        [ErrorText("Incorrect MPConnectionType")]
        D55 = 55,
        [ErrorText("Incorrect MPCapacity")]
        D56 = 56,
        [ErrorText("Incorrect PowerPlant")]
        D57 = 57,
        [ErrorText("No access to the meter")]
        D58 = 58,
        [ErrorText("Incorrect MPTechnologyCode")]
        D59 = 59,
        [ErrorText("Illegal use of code")]
        D60 = 60,
        [ErrorText("Unauthorized Meteringpoint Responsible party")]
        D61 = 61,
        [ErrorText("Incorrect Resolution")]
        D62 = 62,
        [ErrorText("Actual Address indicator is not correct")]
        D63 = 63,
        [ErrorText("Installation not identifiable")]
        E09 = 109,
        [ErrorText("Data not available")]
        E0H = 1000,
        [ErrorText("Unauthorised Grid Access Provider")]
        E0I = 1001,
        [ErrorText("Metering point not identifiable")]
        E10 = 110,
        [ErrorText("Measuring problem")]
        E11 = 111,
        [ErrorText("Other Reason")]
        E14 = 114,
        [ErrorText("Unauthorized balance supplier")]
        E16 = 116,
        [ErrorText("Requested switch date not within time limits")]
        E17 = 117,
        [ErrorText("Unauthorized balance responsible")]
        E18 = 118,
        [ErrorText("Meter readings not within limits")]
        E19 = 119,
        [ErrorText("Metering point blocked for switching")]
        E22 = 122,
        [ErrorText("Product code unknown or not related to MP")]
        E29 = 129,
        [ErrorText("No ongoing switch for MP")]
        E47 = 147,
        [ErrorText("Invalid period")]
        E50 = 150,
        [ErrorText("Invalid number of decimals")]
        E51 = 151,
        [ErrorText("Unathorised metered data responsible")]
        E55 = 155,
        [ErrorText("Already existing relation")]
        E59 = 159,
        [ErrorText("Meter not identifiable")]
        E61 = 161,
        [ErrorText("Incorrect measure unit")]
        E73 = 173,
        [ErrorText("MeteringPoint is not connected")]
        E81 = 181,
        [ErrorText("Incorrect value")]
        E86 = 186,
        [ErrorText("Number of observations dosn't fit observation period/resolution")]
        E87 = 187,
        [ErrorText("Measurement beyond plausibility limits")]
        E90 = 190,
        [ErrorText("Estimate is not acceptable")]
        E91 = 191,
        [ErrorText("Measurement should not be zero")]
        E97 = 197,
        [ErrorText("Measurement has wrong sign")]
        E98 = 198,
    }
}
