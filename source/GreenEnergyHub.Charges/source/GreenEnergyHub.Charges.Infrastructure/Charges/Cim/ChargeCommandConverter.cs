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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml;
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Correlation;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization.Commands;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization.MarketDocument;
using GreenEnergyHub.Messaging.Transport;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Charges.Cim
{
    public class ChargeCommandConverter : DocumentConverter
    {
        private readonly ICorrelationContext _correlationContext;

        public ChargeCommandConverter(
            ICorrelationContext correlationContext,
            IClock clock)
            : base(clock)
        {
            _correlationContext = correlationContext;
        }

        protected override async Task<IInboundMessage> ConvertSpecializedContentAsync(
            [NotNull] XmlReader reader,
            [NotNull] Document document)
        {
            var correlationId = _correlationContext.Id;

            return new ChargeCommand(correlationId)
                {
                    Document = document,
                    ChargeOperation = await ParseChargeOperationAsync(reader).ConfigureAwait(false),
                };
        }

        private static async Task<ChargeOperation> ParseChargeOperationAsync(XmlReader reader)
        {
            var operation = new ChargeOperation();

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeCommandConstants.MarketActivityRecordId, CimChargeCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    operation.Id = content;
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeGroup, CimChargeCommandConstants.Namespace))
                {
                    await ParseChargeGroupIntoOperationAsync(reader, operation).ConfigureAwait(false);
                }
            }

            return operation;
        }

        private static async Task ParseChargeGroupIntoOperationAsync(XmlReader reader, ChargeOperation operation)
        {
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeCommandConstants.ChargeTypeElement, CimChargeCommandConstants.Namespace))
                {
                    await ParseChargeTypeElementIntoOperationAsync(reader, operation).ConfigureAwait(false);
                }
                else if (reader.Is(
                    CimChargeCommandConstants.ChargeGroup,
                    CimChargeCommandConstants.Namespace,
                    XmlNodeType.EndElement))
                {
                    break;
                }
            }
        }

        private static async Task ParseChargeTypeElementIntoOperationAsync(XmlReader reader, ChargeOperation operation)
        {
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeCommandConstants.ChargeOwner, CimChargeCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    operation.ChargeOwner = content;
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeType, CimChargeCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    operation.Type = ChargeTypeMapper.Map(content);
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeId, CimChargeCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    operation.ChargeId = content;
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeName, CimChargeCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    operation.ChargeName = content;
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeDescription, CimChargeCommandConstants.ChargeDescription))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    operation.ChargeDescription = content;
                }
                else if (reader.Is(CimChargeCommandConstants.Resolution, CimChargeCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    operation.Resolution = ResolutionMapper.Map(content);
                }
                else if (reader.Is(
                    CimChargeCommandConstants.ChargeTypeElement,
                    CimChargeCommandConstants.Namespace,
                    XmlNodeType.EndElement))
                {
                    break;
                }
            }
        }

/*
        private static async Task<ChargeLinkDto> ParseChargeLinkAsync(XmlReader reader)
        {
            var link = new ChargeLinkDto();

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
                    link.MeteringPointId = content;
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
                    link.ChargeOwner = content;
                }
                else if (reader.Is(CimChargeLinkCommandConstants.ChargeType, CimChargeLinkCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    link.ChargeType = ChargeTypeMapper.Map(content);
                }
            }

            return link;
        }*/
    }
}
