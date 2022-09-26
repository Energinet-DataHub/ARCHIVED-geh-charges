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

using System;

// ReSharper disable once CheckNamespace - Type is shared so namespace is not determined by project structure/namespace
namespace Energinet.Charges.Contracts.Charge
{
    public record ChargeV1Dto(
        ChargeType ChargeType,
        Resolution Resolution,
        string ChargeId,
        string ChargeName,
        string ChargeOwner,
        string ChargeOwnerName,
        bool TaxIndicator,
        bool TransparentInvoicing,
        DateTime ValidFromDateTime,
        DateTime ValidToDateTime);
}
