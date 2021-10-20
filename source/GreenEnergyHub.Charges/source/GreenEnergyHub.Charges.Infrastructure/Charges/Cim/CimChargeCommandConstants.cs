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

namespace GreenEnergyHub.Charges.Infrastructure.Charges.Cim
{
    internal static class CimChargeCommandConstants
    {
        internal const string Namespace = "urn:ediel.org:structure:requestchangeofpricelist:0:1";

        internal const string MarketActivityRecordId = "mRID";

        internal const string ChargeGroup = "ChargeGroup";

        // CIM contains two kinds of charge types, this is not the one we call charge type, but simply an element name
        internal const string ChargeTypeElement = "ChargeType";

        internal const string ChargeOwner = "chargeTypeOwner_MarketParticipant.mRID";

        internal const string ChargeType = "type";

        internal const string ChargeId = "mRID";

        internal const string ChargeName = "name";

        internal const string ChargeDescription = "description";

        internal const string Resolution = "priceTimeFrame_Period.resolution";
    }
}
