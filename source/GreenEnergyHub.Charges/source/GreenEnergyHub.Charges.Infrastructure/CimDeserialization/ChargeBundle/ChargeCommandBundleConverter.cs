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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.SchemaValidation;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.CimDeserialization.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using GreenEnergyHub.Iso8601;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.CimDeserialization.ChargeBundle
{
    public class ChargeCommandBundleConverter : DocumentConverter, IChargeCommandBundleConverter
    {
        private readonly IIso8601Durations _iso8601Durations;

        public ChargeCommandBundleConverter(
            IClock clock,
            IIso8601Durations iso8601Durations)
            : base(clock)
        {
            _iso8601Durations = iso8601Durations;
        }

        protected override async Task<ChargeCommandBundle> ConvertSpecializedContentAsync(
            SchemaValidatingReader reader,
            DocumentDto document)
        {
            var processType = document.BusinessReasonCode;
            switch (document.BusinessReasonCode)
            {
                case BusinessReasonCode.UpdateChargeInformation:
                    return await ParseChargeInformationCommandBundleAsync(reader, document).ConfigureAwait(false);
                case BusinessReasonCode.UpdateChargePrices:
                    return await ParseChargePriceCommandBundleAsync(reader, document).ConfigureAwait(false);
                case BusinessReasonCode.Unknown:
                case BusinessReasonCode.UpdateMasterDataSettlement:
                default:
                    throw new InvalidOperationException(
                        $"Could not convert specialized content. Process type not supported: {processType}");
            }
        }

        private async Task<ChargeInformationCommandBundle> ParseChargeInformationCommandBundleAsync(
            SchemaValidatingReader reader, DocumentDto document)
        {
            var chargeOperationsAsync = await ParseChargeInformationOperationsAsync(reader).ConfigureAwait(false);
            var chargeCommands = chargeOperationsAsync
                .GroupBy(x => new { x.ChargeId, x.ChargeOwner, x.Type })
                .Select(chargeOperationDtoGroup =>
                    new ChargeInformationCommand(
                        document,
                        chargeOperationDtoGroup.AsEnumerable().Select(dto => dto).ToList()))
                .ToList();
            return new ChargeInformationCommandBundle(document, chargeCommands);
        }

        private async Task<ChargePriceCommandBundle> ParseChargePriceCommandBundleAsync(
            SchemaValidatingReader reader, DocumentDto document)
        {
            var priceOperations = await ParseChargePriceOperationsAsync(reader).ConfigureAwait(false);
            var priceCommands = priceOperations
                .GroupBy(x => new { x.ChargeId, x.ChargeOwner, x.Type })
                .Select(group =>
                    new ChargePriceCommand(
                        document,
                        priceOperations.AsEnumerable().Select(dto => dto).ToList()))
                .ToList();
            return new ChargePriceCommandBundle(document, priceCommands);
        }

        private async Task<List<ChargePriceOperationDto>> ParseChargePriceOperationsAsync(SchemaValidatingReader reader)
        {
            var operations = new List<ChargePriceOperationDto>();
            var operationId = string.Empty;

            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeCommandConstants.Id))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    operationId = content;
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeGroup))
                {
                    while (await reader.AdvanceAsync().ConfigureAwait(false))
                    {
                        if (reader.Is(CimChargeCommandConstants.ChargeTypeElement))
                        {
                            var operation = await ParseChargePriceOperationAsync(reader, operationId).ConfigureAwait(false);
                            operations.Add(operation);
                        }
                        else if (reader.Is(CimChargeCommandConstants.ChargeGroup, NodeType.EndElement))
                        {
                            break;
                        }
                    }
                }
            }

            return operations;
        }

        private async Task<ChargePriceOperationDto> ParseChargePriceOperationAsync(SchemaValidatingReader reader, string operationId)
        {
            var senderProvidedChargeId = string.Empty;
            var chargeOwner = string.Empty;
            var chargeType = ChargeType.Unknown;
            var resolution = Resolution.Unknown;
            Instant startDateTime = default;
            Instant endDateTime = default;
            Instant pointsStartTime = default;
            Instant pointsEndTime = default;
            var points = new List<Point>();

            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeCommandConstants.ChargeOwner))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    chargeOwner = content;
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeType))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    chargeType = ChargeTypeMapper.Map(content);
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeId))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    senderProvidedChargeId = content;
                }
                else if (reader.Is(CimChargeCommandConstants.StartDateTime))
                {
                    startDateTime = await reader.ReadValueAsNodaTimeAsync().ConfigureAwait(false);
                }
                else if (reader.Is(CimChargeCommandConstants.EndDateTime))
                {
                    endDateTime = await reader.ReadValueAsNodaTimeAsync().ConfigureAwait(false);
                }
                else if (reader.Is(CimChargeCommandConstants.SeriesPeriod))
                {
                    var seriesPeriodIntoOperationAsync = await ParseSeriesPeriodIntoOperationAsync(reader, startDateTime).ConfigureAwait(false);
                    points.AddRange(seriesPeriodIntoOperationAsync.Points);
                    resolution = seriesPeriodIntoOperationAsync.Resolution;
                    pointsStartTime = seriesPeriodIntoOperationAsync.IntervalStartTime;
                    pointsEndTime = seriesPeriodIntoOperationAsync.IntervalEndTime;
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeTypeElement, NodeType.EndElement))
                {
                    break;
                }
            }

            return new ChargePriceOperationDto(
                operationId,
                chargeType,
                senderProvidedChargeId,
                chargeOwner,
                startDateTime,
                endDateTime,
                pointsStartTime,
                pointsEndTime,
                resolution,
                points);
        }

        private async Task<List<ChargeOperationDto>> ParseChargeInformationOperationsAsync(SchemaValidatingReader reader)
        {
            var operations = new List<ChargeOperationDto>();
            var operationId = string.Empty;

            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeCommandConstants.Id))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    operationId = content;
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeGroup))
                {
                    operations.Add(await ParseChargeGroupIntoOperationAsync(reader, operationId).ConfigureAwait(false));
                }
            }

            return operations;
        }

        private async Task<ChargeOperationDto> ParseChargeGroupIntoOperationAsync(SchemaValidatingReader reader, string operationId)
        {
            ChargeOperationDto? operation = null;
            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeCommandConstants.ChargeTypeElement))
                {
                    operation = await ParseChargeTypeElementIntoOperationAsync(reader, operationId).ConfigureAwait(false);
                }
                else if (reader.Is(
                    CimChargeCommandConstants.ChargeGroup,
                    NodeType.EndElement))
                {
                    break;
                }
            }

            return operation!;
        }

        private async Task<ChargeOperationDto> ParseChargeTypeElementIntoOperationAsync(SchemaValidatingReader reader, string operationId)
        {
            var chargeOwner = string.Empty;
            var chargeType = ChargeType.Unknown;
            var senderProvidedChargeId = string.Empty;
            var chargeName = string.Empty;
            var description = string.Empty;
            var resolution = Resolution.Unknown;
            var priceResolution = Resolution.Unknown;
            Instant startDateTime = default;
            Instant? endDateTime = null;
            var vatClassification = VatClassification.Unknown;
            var transparentInvoicing = TransparentInvoicing.Unknown;
            var taxIndicator = TaxIndicator.Unknown;
            Instant pointsStartTime = default;
            Instant pointsEndTime = default;
            var points = new List<Point>();

            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeCommandConstants.ChargeOwner) && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    chargeOwner = content;
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeType))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    chargeType = ChargeTypeMapper.Map(content);
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeId) && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    senderProvidedChargeId = content;
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeName) && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    chargeName = content;
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeDescription) && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    description = content;
                }
                else if (reader.Is(CimChargeCommandConstants.Resolution))
                {
                    // Note: Resolution can be set two places in the file. If its filled here, that the one that will be used.
                    // This is done to be able to handle changes to charges without prices
                    var content = await reader.ReadValueAsDurationAsync().ConfigureAwait(false);
                    resolution = ResolutionMapper.Map(content);
                }
                else if (reader.Is(CimChargeCommandConstants.StartDateTime))
                {
                    startDateTime = await reader.ReadValueAsNodaTimeAsync().ConfigureAwait(false);
                }
                else if (reader.Is(CimChargeCommandConstants.EndDateTime))
                {
                    endDateTime = await reader.ReadValueAsNodaTimeAsync().ConfigureAwait(false);
                }
                else if (reader.Is(CimChargeCommandConstants.VatClassification) && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    vatClassification = VatClassificationMapper.Map(content);
                }
                else if (reader.Is(CimChargeCommandConstants.TransparentInvoicing) && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsBoolAsync().ConfigureAwait(false);
                    transparentInvoicing = TransparentInvoicingMapper.Map(content);
                }
                else if (reader.Is(CimChargeCommandConstants.TaxIndicator) && reader.CanReadValue)
                {
                    var content = await reader.ReadValueAsBoolAsync().ConfigureAwait(false);
                    taxIndicator = TaxIndicatorMapper.Map(content);
                }
                else if (reader.Is(CimChargeCommandConstants.SeriesPeriod))
                {
                    var seriesPeriodIntoOperationAsync = await ParseSeriesPeriodIntoOperationAsync(reader, startDateTime).ConfigureAwait(false);
                    points.AddRange(seriesPeriodIntoOperationAsync.Points);
                    priceResolution = seriesPeriodIntoOperationAsync.Resolution;
                    pointsStartTime = seriesPeriodIntoOperationAsync.IntervalStartTime;
                    pointsEndTime = seriesPeriodIntoOperationAsync.IntervalEndTime;
                }
                else if (reader.Is(CimChargeCommandConstants.ChargeTypeElement, NodeType.EndElement))
                {
                    break;
                }
            }

            return new ChargeOperationDto(
                operationId,
                chargeType,
                senderProvidedChargeId,
                chargeName,
                description,
                chargeOwner,
                resolution,
                priceResolution,
                taxIndicator,
                transparentInvoicing,
                vatClassification,
                startDateTime,
                endDateTime,
                pointsStartTime,
                pointsEndTime,
                points);
        }

        private async Task<ParseSeriesPeriodResult> ParseSeriesPeriodIntoOperationAsync(SchemaValidatingReader reader, Instant startDateTime)
        {
            var points = new List<Point>();
            var priceResolution = Resolution.Unknown;
            Instant endDateTime = default;

            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeCommandConstants.PriceResolution))
                {
                    var content = await reader.ReadValueAsDurationAsync().ConfigureAwait(false);
                    priceResolution = ResolutionMapper.Map(content);
                }
                else if (reader.Is(CimChargeCommandConstants.TimeInterval))
                {
                    (startDateTime, endDateTime) = await ParseTimeIntervalAsync(reader, startDateTime)
                        .ConfigureAwait(false);
                }
                else if (reader.Is(CimChargeCommandConstants.Point))
                {
                    var point = await ParsePointAsync(reader, priceResolution, startDateTime).ConfigureAwait(false);
                    points.Add(point);
                }
                else if (reader.Is(
                    CimChargeCommandConstants.SeriesPeriod,
                    NodeType.EndElement))
                {
                    break;
                }
            }

            return new ParseSeriesPeriodResult(points, priceResolution, startDateTime, endDateTime);
        }

        private static async Task<(Instant StartDateTime, Instant EndDateTime)> ParseTimeIntervalAsync(SchemaValidatingReader reader, Instant intervalStartDateTime)
        {
            Instant intervalEndDateTime = default;
            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeCommandConstants.TimeIntervalStart))
                {
                    var cimTimeInterval = await reader
                        .ReadValueAsStringAsync()
                        .ConfigureAwait(false);

                    intervalStartDateTime = Instant.FromDateTimeOffset(DateTimeOffset.Parse(cimTimeInterval));
                }
                else if (reader.Is(CimChargeCommandConstants.TimeIntervalEnd))
                {
                    var cimTimeInterval = await reader
                        .ReadValueAsStringAsync()
                        .ConfigureAwait(false);

                    intervalEndDateTime = Instant.FromDateTimeOffset(DateTimeOffset.Parse(cimTimeInterval));
                }
                else if (reader.Is(CimChargeCommandConstants.TimeInterval, NodeType.EndElement))
                {
                    break;
                }
            }

            return (intervalStartDateTime, intervalEndDateTime);
        }

        private async Task<Point> ParsePointAsync(SchemaValidatingReader reader, Resolution resolution, Instant startDateTime)
        {
            int position = 0;
            decimal price = 0m;
            Instant time = default;

            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (reader.Is(CimChargeCommandConstants.Position))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    position = int.Parse(content, CultureInfo.InvariantCulture);
                }
                else if (reader.Is(CimChargeCommandConstants.Price))
                {
                    var content = await reader.ReadValueAsStringAsync().ConfigureAwait(false);
                    price = decimal.Parse(content, CultureInfo.InvariantCulture);
                }
                else if (reader.Is(
                    CimChargeCommandConstants.Point,
                    NodeType.EndElement))
                {
                    time = _iso8601Durations.GetTimeFixedToDuration(
                        startDateTime,
                        ResolutionMapper.Map(resolution),
                        position - 1);
                    break;
                }
            }

            return new Point(position, price, time);
        }
    }
}
