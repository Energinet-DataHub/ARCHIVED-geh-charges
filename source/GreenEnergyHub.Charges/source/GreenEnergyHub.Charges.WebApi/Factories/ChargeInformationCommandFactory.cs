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
using System.Collections.Generic;
using Energinet.DataHub.Charges.Contracts.Charge;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using NodaTime;
using NodaTime.Extensions;
using ChargeType = GreenEnergyHub.Charges.Domain.Charges.ChargeType;
using Resolution = GreenEnergyHub.Charges.Domain.Charges.Resolution;
using VatClassification = GreenEnergyHub.Charges.Domain.Charges.VatClassification;

namespace GreenEnergyHub.Charges.WebApi.Factories;

public class ChargeInformationCommandFactory : IChargeInformationCommandFactory
{
    private readonly IClock _clock;
    private readonly string _meteringPointAdministratorGln;

    public ChargeInformationCommandFactory(IClock clock, string meteringPointAdministratorGln)
    {
        _clock = clock;
        _meteringPointAdministratorGln = meteringPointAdministratorGln;
    }

    public ChargeInformationCommand Create(CreateChargeV1Dto charge)
    {
        var operations = CreateOperation(charge);
        var document = CreateDocument(charge);

        return new ChargeInformationCommand(document, operations);
    }

    private DocumentDto CreateDocument(CreateChargeV1Dto charge)
    {
        var documentId = $"geh{Guid.NewGuid():N}";

        var document = new DocumentDto(
            documentId,
            _clock.GetCurrentInstant(),
            DocumentType.RequestChangeOfPriceList,
            _clock.GetCurrentInstant(),
            new MarketParticipantDto(
                Guid.Empty,
                charge.SenderMarketParticipant.MarketParticipantId,
                MarketParticipantRoleMapper.Map(charge.SenderMarketParticipant.BusinessProcessRole),
                Guid.Empty),
            new MarketParticipantDto(
                Guid.Empty,
                _meteringPointAdministratorGln,
                MarketParticipantRole.MeteringPointAdministrator,
                Guid.Empty),
            IndustryClassification.Electricity,
            BusinessReasonCode.UpdateChargeInformation);

        return document;
    }

    private static IReadOnlyCollection<ChargeInformationOperationDto> CreateOperation(CreateChargeV1Dto charge)
    {
        var operationId = $"{Guid.NewGuid():N}";

        IReadOnlyCollection<ChargeInformationOperationDto> operations = new List<ChargeInformationOperationDto>
        {
            new ChargeInformationOperationDto(
                operationId,
                (ChargeType)charge.ChargeType,
                charge.SenderProvidedChargeId,
                charge.ChargeName,
                charge.Description,
                charge.SenderMarketParticipant.MarketParticipantId,
                (Resolution)charge.Resolution,
                charge.TaxIndicator ? TaxIndicator.Tax : TaxIndicator.NoTax,
                charge.TransparentInvoicing ? TransparentInvoicing.Transparent : TransparentInvoicing.NonTransparent,
                (VatClassification)charge.VatClassification,
                charge.EffectiveDate.UtcDateTime.ToInstant(),
                null),
        };

        return operations;
    }
}
