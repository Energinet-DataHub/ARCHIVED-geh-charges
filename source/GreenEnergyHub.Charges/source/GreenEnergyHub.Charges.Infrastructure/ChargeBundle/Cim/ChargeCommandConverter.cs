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
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.ChargeLinkBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.Correlation;
using GreenEnergyHub.Charges.Infrastructure.MarketDocument.Cim;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.Messaging.Transport;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.ChargeBundle.Cim
{
    public class ChargeCommandConverter : DocumentConverter
    {
        private readonly ICorrelationContext _correlationContext;
        private readonly IIso8601Durations _iso8601Durations;

        public ChargeCommandConverter(
            ICorrelationContext correlationContext,
            IClock clock,
            IIso8601Durations iso8601Durations)
            : base(clock)
        {
            _correlationContext = correlationContext;
            _iso8601Durations = iso8601Durations;
        }

        protected override async Task<IInboundMessage> ConvertSpecializedContentAsync(
            [NotNull] XmlReader reader,
            [NotNull] DocumentDto document)
        {
            var correlationId = _correlationContext.Id;

            return new ChargeCommand(correlationId)
                {
                    Document = document,
                    ChargeOperation = await ParseChargeOperationAsync(reader).ConfigureAwait(false),
                };
        }

        private async Task<ChargeOperation> ParseChargeOperationAsync(XmlReader reader)
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

        private async Task ParseChargeGroupIntoOperationAsync(XmlReader reader, ChargeOperation operation)
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

        private async Task ParseChargeTypeElementIntoOperationAsync(XmlReader reader, ChargeOperation operation)
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
                else if (reader.Is(CimChargeCommandConstants.ChargeDescription, CimChargeCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    operation.ChargeDescription = content;
                }
                else if (reader.Is(CimChargeCommandConstants.Resolution, CimChargeCommandConstants.Namespace))
                {
                    // Note: Resolution can be set two places in the file. If its filled here, that the one that will be used.
                    // This is done to be able to handle changes to charges without prices
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    operation.Resolution = ResolutionMapper.Map(content);
                }
                else if (reader.Is(CimChargeCommandConstants.StartDateTime, CimChargeCommandConstants.Namespace))
                {
                    operation.StartDateTime = Instant.FromDateTimeUtc(reader.ReadElementContentAsDateTime());
                }
                else if (reader.Is(CimChargeCommandConstants.EndDateTime, CimChargeCommandConstants.Namespace))
                {
                    operation.EndDateTime = Instant.FromDateTimeUtc(reader.ReadElementContentAsDateTime());
                }
                else if (reader.Is(CimChargeCommandConstants.VatClassification, CimChargeCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    operation.VatClassification = VatClassificationMapper.Map(content);
                }
                else if (reader.Is(CimChargeCommandConstants.TransparentInvoicing, CimChargeCommandConstants.Namespace))
                {
                    operation.TransparentInvoicing = reader.ReadElementContentAsBoolean();
                }
                else if (reader.Is(CimChargeCommandConstants.TaxIndicator, CimChargeCommandConstants.Namespace))
                {
                    operation.TaxIndicator = reader.ReadElementContentAsBoolean();
                }
                else if (reader.Is(CimChargeCommandConstants.SeriesPeriod, CimChargeCommandConstants.Namespace))
                {
                    await ParseSeriesPeriodIntoOperationAsync(reader, operation).ConfigureAwait(false);
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

        private async Task ParseSeriesPeriodIntoOperationAsync(XmlReader reader, ChargeOperation operation)
        {
            // We use the effective start date time unless the period is later specified
            var startDateTime = operation.StartDateTime;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeCommandConstants.PeriodResolution, CimChargeCommandConstants.Namespace))
                {
                    // Note, this is the second place where the resolution might be identified
                    // If it was not set previous, we use this one instead
                    if (operation.Resolution == Resolution.Unknown)
                    {
                        var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        operation.Resolution = ResolutionMapper.Map(content);
                    }
                }
                else if (reader.Is(CimChargeCommandConstants.TimeInterval, CimChargeCommandConstants.Namespace))
                {
                    startDateTime = await ParseTimeIntervalAsync(reader, startDateTime).ConfigureAwait(false);
                }
                else if (reader.Is(CimChargeCommandConstants.Point, CimChargeCommandConstants.Namespace))
                {
                    var point = await ParsePointAsync(reader, operation, startDateTime).ConfigureAwait(false);
                    operation.Points.Add(point);
                }
                else if (reader.Is(
                    CimChargeCommandConstants.SeriesPeriod,
                    CimChargeCommandConstants.Namespace,
                    XmlNodeType.EndElement))
                {
                    break;
                }
            }
        }

        private async Task<Instant> ParseTimeIntervalAsync(XmlReader reader, Instant startDateTime)
        {
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeCommandConstants.TimeIntervalStart, CimChargeCommandConstants.Namespace))
                {
                    return Instant.FromDateTimeUtc(reader.ReadElementContentAsDateTime());
                }
                else if (reader.Is(
                    CimChargeCommandConstants.TimeInterval,
                    CimChargeCommandConstants.Namespace,
                    XmlNodeType.EndElement))
                {
                    break;
                }
            }

            return startDateTime;
        }

        private async Task<Point> ParsePointAsync(XmlReader reader, ChargeOperation operation, Instant startDateTime)
        {
            var point = new Point();

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeCommandConstants.Position, CimChargeCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    point.Position = int.Parse(content, CultureInfo.InvariantCulture);
                }
                else if (reader.Is(CimChargeCommandConstants.Price, CimChargeCommandConstants.Namespace))
                {
                    var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    point.Price = decimal.Parse(content, CultureInfo.InvariantCulture);
                }
                else if (reader.Is(
                    CimChargeCommandConstants.Point,
                    CimChargeCommandConstants.Namespace,
                    XmlNodeType.EndElement))
                {
                    point.Time = _iso8601Durations.AddDuration(
                        startDateTime,
                        ResolutionMapper.Map(operation.Resolution),
                        point.Position - 1);
                    break;
                }
            }

            return point;
        }
    }
}
