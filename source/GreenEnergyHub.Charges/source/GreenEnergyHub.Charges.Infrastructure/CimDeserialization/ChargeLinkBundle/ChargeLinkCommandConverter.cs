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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using Energinet.DataHub.Core.Messaging.Transport;
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
            [NotNull] XmlReader reader,
            [NotNull] DocumentDto document)
        {
            return await ParseChargeLinksCommandAsync(reader, document);
        }

        private static async Task<ChargeLinksCommand> ParseChargeLinksCommandAsync(XmlReader reader, DocumentDto documentDto)
        {
            var link = new ChargeLinkDto();
            string meteringPointId = null!;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeLinkCommandConstants.Id, CimChargeLinkCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    link.OperationId = content;
                }
                else if (reader.Is(CimChargeLinkCommandConstants.MeteringPointId, CimChargeLinkCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    meteringPointId = content;
                }
                else if (reader.Is(CimChargeLinkCommandConstants.StartDateTime, CimChargeLinkCommandConstants.Namespace))
                {
                    link.StartDateTime = Instant.FromDateTimeUtc(reader.ReadElementContentAsDateTime());
                }
                else if (reader.Is(CimChargeLinkCommandConstants.EndDateTime, CimChargeLinkCommandConstants.Namespace))
                {
                    link.EndDateTime = Instant.FromDateTimeUtc(reader.ReadElementContentAsDateTime());
                }
                else if (reader.Is(CimChargeLinkCommandConstants.ChargeId, CimChargeLinkCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    link.SenderProvidedChargeId = content;
                }
                else if (reader.Is(CimChargeLinkCommandConstants.Factor, CimChargeLinkCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    link.Factor = int.Parse(content, CultureInfo.InvariantCulture);
                }
                else if (reader.Is(CimChargeLinkCommandConstants.ChargeOwner, CimChargeLinkCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    link.ChargeOwnerId = content;
                }
                else if (reader.Is(CimChargeLinkCommandConstants.ChargeType, CimChargeLinkCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    link.ChargeType = ChargeTypeMapper.Map(content);
                }
            }

            return new ChargeLinksCommand(meteringPointId, documentDto, new List<ChargeLinkDto> { link });
        }
    }
}
