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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class ChargeLinksCommandBuilder
    {
        private readonly DocumentDto _document = new()
        {
            Id = Guid.NewGuid().ToString(),
            CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
            BusinessReasonCode = BusinessReasonCode.UpdateMasterDataSettlement,
            IndustryClassification = IndustryClassification.Electricity,
            RequestDate = SystemClock.Instance.GetCurrentInstant(),
            Recipient = new MarketParticipantDto
            {
                Id = Guid.NewGuid().ToString("N"),
                BusinessProcessRole = MarketParticipantRole.GridAccessProvider,
            },
            Sender = new MarketParticipantDto
            {
                Id = Guid.NewGuid().ToString("N"),
                BusinessProcessRole = MarketParticipantRole.MeteringPointAdministrator,
            },
            Type = DocumentType.RequestChangeBillingMasterData,
        };

        private List<ChargeLinkDto> _links = new();

        public ChargeLinksCommandBuilder WithChargeLinks(List<ChargeLinkDto> links)
        {
            _links = links;
            return this;
        }

        public ChargeLinksCommand Build()
        {
            return new ChargeLinksCommand(_document, _links);
        }
    }
}
