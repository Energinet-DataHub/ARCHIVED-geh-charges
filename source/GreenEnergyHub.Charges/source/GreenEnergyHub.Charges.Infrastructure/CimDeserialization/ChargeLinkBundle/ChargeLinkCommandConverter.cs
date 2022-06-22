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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Messaging.Transport;
using Energinet.DataHub.Core.SchemaValidation;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.CimDeserialization.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.CimDeserialization.ChargeLinkBundle
{
    public class ChargeLinkCommandConverter : DocumentConverter
    {
        public ChargeLinkCommandConverter(IClock clock)
            : base(clock)
        {
        }

        protected override async Task<IInboundMessage> ConvertSpecializedContentAsync(
            SchemaValidatingReader reader,
            DocumentDto document)
        {
            var chargeLinksCommands = await ParseChargeLinkCommandsAsync(reader, document).ConfigureAwait(false);

            return new ChargeLinksBundleDto(chargeLinksCommands);
        }

        private static async Task<IReadOnlyCollection<ChargeLinksCommand>> ParseChargeLinkCommandsAsync(
            SchemaValidatingReader reader,
            DocumentDto document)
        {
            var chargeLinks = new List<ChargeLinksCommand>();

            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                var chargeLinkCommandAsync = await ParseChargeLinkCommandAsync(reader, document).ConfigureAwait(false);
                chargeLinks.Add(chargeLinkCommandAsync);

                await reader
                    .ReadUntilEoFOrNextElementNameAsync(CimChargeLinkCommandConstants.MktActivityRecord)
                    .ConfigureAwait(false);
            }

            return chargeLinks.AsReadOnly();
        }

        private static async Task<ChargeLinksCommand> ParseChargeLinkCommandAsync(
            SchemaValidatingReader reader,
            DocumentDto document)
        {
            var chargeLinkDtos = new List<ChargeLinkDto>();
            var operationId = string.Empty;
            string meteringPointId = null!;

            do
            {
                if (reader.Is(CimChargeLinkCommandConstants.Id))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    operationId = content;
                }
                else if (reader.Is(CimChargeLinkCommandConstants.MeteringPointId))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    meteringPointId = content;
                }
                else if (reader.Is(CimChargeLinkCommandConstants.ChargeGroup))
                {
                    chargeLinkDtos.Add(await ParseChargeGroupIntoChargeLinkDtoAsync(reader, operationId, meteringPointId)
                        .ConfigureAwait(false));
                }
                else if (reader.Is(CimChargeLinkCommandConstants.MktActivityRecord, NodeType.EndElement))
                {
                    // For now we break on MktActivityRecord to only have one ChargeLinkDto for each ChargeLinksCommand
                    break;
                }
            }
            while (await reader.AdvanceAsync().ConfigureAwait(false));

            return new ChargeLinksCommand(document, chargeLinkDtos);
        }

        private static async Task<ChargeLinkDto> ParseChargeGroupIntoChargeLinkDtoAsync(
            SchemaValidatingReader reader,
            string operationId,
            string meteringPointId)
        {
            ChargeLinkDto? chargeLinkDto = null;
            do
            {
                if (reader.Is(CimChargeLinkCommandConstants.ChargeType))
                {
                    chargeLinkDto = await ParseChargeTypeElementIntoChargeLinkDtoAsync(reader, operationId, meteringPointId)
                        .ConfigureAwait(false);
                }
                else if (reader.Is(CimChargeLinkCommandConstants.ChargeGroup, NodeType.EndElement))
                {
                    break;
                }
            }
            while (await reader.AdvanceAsync().ConfigureAwait(false));

            return chargeLinkDto!;
        }

        private static async Task<ChargeLinkDto> ParseChargeTypeElementIntoChargeLinkDtoAsync(
            SchemaValidatingReader reader,
            string operationId,
            string meteringPointId)
        {
            Instant startDateTime = default;
            Instant? endDateTime = null;
            string senderProvidedChargeId = null!;
            var factor = 0;
            string chargeOwner = null!;
            var chargeType = ChargeType.Unknown;

            do
            {
                if (reader.Is(CimChargeLinkCommandConstants.EffectiveDate))
                {
                    startDateTime = await reader.ReadValueAsNodaTimeAsync().ConfigureAwait(false);
                }
                else if (reader.Is(CimChargeLinkCommandConstants.TerminationDate))
                {
                    endDateTime = await reader.ReadValueAsNodaTimeAsync().ConfigureAwait(false);
                }
                else if (reader.Is(CimChargeLinkCommandConstants.ChargeId))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    senderProvidedChargeId = content;
                }
                else if (reader.Is(CimChargeLinkCommandConstants.Factor))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    factor = int.Parse(content, CultureInfo.InvariantCulture);
                }
                else if (reader.Is(CimChargeLinkCommandConstants.ChargeOwner))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    chargeOwner = content;
                }
                else if (reader.Is(CimChargeLinkCommandConstants.Type))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    chargeType = ChargeTypeMapper.Map(content);
                }
                else if (reader.Is(CimChargeLinkCommandConstants.ChargeType, NodeType.EndElement))
                {
                    break;
                }
            }
            while (await reader.AdvanceAsync().ConfigureAwait(false));

            return new ChargeLinkDto(
                operationId,
                meteringPointId,
                startDateTime,
                endDateTime,
                senderProvidedChargeId,
                factor,
                chargeOwner,
                chargeType);
        }
    }
}
