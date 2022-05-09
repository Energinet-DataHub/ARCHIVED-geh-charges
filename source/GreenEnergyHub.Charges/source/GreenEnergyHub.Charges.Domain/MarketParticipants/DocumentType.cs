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

namespace GreenEnergyHub.Charges.Domain.MarketParticipants
{
    /// <summary>
    /// The document type indicates the intended business context of this business message.
    /// </summary>
    public enum DocumentType
    {
        Unknown = 0,
        RequestChangeBillingMasterData = 1, // Document requesting to update links between metering points and charges
        RequestChangeOfPriceList = 2, // Document requesting to update charge master data and/or prices
        NotifyBillingMasterData = 3, // Document containing changes to links between metering points and charges
        NotifyPriceList = 4, // Document containing changes to charge master data or prices
        ConfirmRequestChangeOfPriceList = 7, // Document containing confirmation(s) for charges
        RejectRequestChangeOfPriceList = 8, // Document containing rejection(s) for charges
        ConfirmRequestChangeBillingMasterData = 9, // Document containing confirmation(s) for charge links
        RejectRequestChangeBillingMasterData = 10, // Document containing rejection(s) for charge links
    }
}
