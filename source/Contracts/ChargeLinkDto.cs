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

namespace Energinet.Charges.Contracts
{
    /// <summary>
    /// TODO.
    /// </summary>
    public record ChargeLinkDto(

        // The type of charge; tariff, fee or subscription
        ChargeType ChargeType,

        // A charge identifier provided by the market participant. Combined with charge owner and charge type it becomes unique
        string ChargeId,

        // Charge name provided by the market participant.
        string ChargeName,

        // A charge owner identification, e.g. the market participant's GLN or EIC number
        string ChargeOwnerId,

        // The market participant's company name
        string ChargeOwnerName,

        // Indicates whether a tariff is considered a tax or not
        bool TaxIndicator,

        // Indicates whether the charge owner wants the charge to be displayed on the customer invoice
        bool TransparentInvoicing,

        // Also known as quantity
        int Factor,
        DateTime StartDateTimeUtc,
        DateTime? EndDateTimeUtc);
}
