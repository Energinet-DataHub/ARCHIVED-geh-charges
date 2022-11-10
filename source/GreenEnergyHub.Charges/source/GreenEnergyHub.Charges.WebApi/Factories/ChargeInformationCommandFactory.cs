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
    private static IClock _clock;

    public ChargeInformationCommandFactory(IClock clock)
    {
        _clock = clock;
    }

    public ChargeInformationCommand Create(CreateChargeInformationV1Dto chargeInformation)
    {
        var operations = CreateOperation(chargeInformation);
        var document = CreateDocument(chargeInformation);

        return new ChargeInformationCommand(document, operations);
    }

    private static DocumentDto CreateDocument(CreateChargeInformationV1Dto chargeInformation)
    {
        var documentId = "dh" + Guid.NewGuid().ToString("N");

        var document = new DocumentDto(
            documentId,
            _clock.GetCurrentInstant(),
            DocumentType.RejectRequestChangeOfPriceList,
            _clock.GetCurrentInstant(),
            new MarketParticipantDto(
                Guid.NewGuid(),
                chargeInformation.SenderMarketParticipant.MarketParticipantId,
                MarketParticipantRoleMapper.Map(chargeInformation.SenderMarketParticipant.BusinessProcessRole),
                Guid.Empty),
            new MarketParticipantDto(
                Guid.NewGuid(),
                MarketParticipantConstants.MeteringPointAdministratorGln,
                MarketParticipantRole.MeteringPointAdministrator,
                Guid.Empty),
            IndustryClassification.Electricity,
            BusinessReasonCode.UpdateChargeInformation);

        return document;
    }

    private static IReadOnlyCollection<ChargeInformationOperationDto> CreateOperation(CreateChargeInformationV1Dto chargeInformation)
    {
        var operationId = Guid.NewGuid().ToString("N");

        IReadOnlyCollection<ChargeInformationOperationDto> operations = new List<ChargeInformationOperationDto>
        {
            new ChargeInformationOperationDto(
                operationId,
                (ChargeType)chargeInformation.ChargeType,
                chargeInformation.SenderProvidedChargeId,
                chargeInformation.ChargeName,
                chargeInformation.Description,
                chargeInformation.SenderMarketParticipant.MarketParticipantId,
                (Resolution)chargeInformation.Resolution,
                chargeInformation.TaxIndicator ? TaxIndicator.Tax : TaxIndicator.NoTax,
                chargeInformation.TransparentInvoicing ? TransparentInvoicing.Transparent : TransparentInvoicing.NonTransparent,
                (VatClassification)chargeInformation.VatClassification,
                chargeInformation.EffectiveDate.ToInstant(),
                null),
        };

        return operations;
    }
}
