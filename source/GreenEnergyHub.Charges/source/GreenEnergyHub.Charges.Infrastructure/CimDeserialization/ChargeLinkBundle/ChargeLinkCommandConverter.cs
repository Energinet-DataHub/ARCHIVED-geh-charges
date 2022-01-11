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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Messaging.Transport;
using Energinet.DataHub.Core.SchemaValidation;
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

            return new ChargeLinksCommandBundle(chargeLinksCommands);
        }

        private static async Task<List<ChargeLinksCommand>> ParseChargeLinkCommandsAsync(SchemaValidatingReader reader, DocumentDto document)
        {
            var chargeLinks = new List<ChargeLinksCommand>();

            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                var chargeLinkCommandAsync = await ParseChargeLinkCommandAsync(reader, document).ConfigureAwait(false);
                chargeLinks.Add(chargeLinkCommandAsync);

                await reader
                    .ReadUntilEoFOrNextElementNameAsync(
                        CimChargeLinkCommandConstants.MktActivityRecord);
            }

            return chargeLinks;
        }

        private static async Task<ChargeLinksCommand> ParseChargeLinkCommandAsync(SchemaValidatingReader reader, DocumentDto document)
        {
            var link = new ChargeLinkDto();
            string meteringPointId = null!;

            do
            {
                if (reader.Is(CimChargeLinkCommandConstants.Id))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    link.OperationId = content;
                }
                else if (reader.Is(CimChargeLinkCommandConstants.MeteringPointId))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    meteringPointId = content;
                }
                else if (reader.Is(CimChargeLinkCommandConstants.StartDateTime))
                {
                    link.StartDateTime = await reader.ReadValueAsNodaTimeAsync().ConfigureAwait(false);
                }
                else if (reader.Is(CimChargeLinkCommandConstants.EndDateTime))
                {
                    link.EndDateTime = await reader.ReadValueAsNodaTimeAsync().ConfigureAwait(false);
                }
                else if (reader.Is(CimChargeLinkCommandConstants.ChargeId))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    link.SenderProvidedChargeId = content;
                }
                else if (reader.Is(CimChargeLinkCommandConstants.Factor))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    link.Factor = int.Parse(content, CultureInfo.InvariantCulture);
                }
                else if (reader.Is(CimChargeLinkCommandConstants.ChargeOwner))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    link.ChargeOwnerId = content;
                }
                else if (reader.Is(CimChargeLinkCommandConstants.ChargeType))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    link.ChargeType = ChargeTypeMapper.Map(content);
                }
                else if (reader.Is(
                             CimChargeLinkCommandConstants.MktActivityRecord,
                             NodeType.EndElement))
                {
                    break;
                }
            }
            while (await reader.AdvanceAsync().ConfigureAwait(false));

            return new ChargeLinksCommand(meteringPointId, document, new List<ChargeLinkDto> { link });
        }
    }
}
