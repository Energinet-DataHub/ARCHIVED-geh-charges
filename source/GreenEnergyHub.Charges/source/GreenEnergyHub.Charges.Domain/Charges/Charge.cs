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
using System.Linq;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges.Exceptions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Charges
{
    public class Charge
    {
        private readonly List<Point> _points;
        private readonly List<ChargePeriod> _periods;

        private Charge(
            Guid id,
            string senderProvidedChargeId,
            Guid ownerId,
            ChargeType type,
            Resolution resolution,
            bool taxIndicator,
            List<Point> points,
            List<ChargePeriod> periods)
        {
            Id = id;
            SenderProvidedChargeId = senderProvidedChargeId;
            OwnerId = ownerId;
            Type = type;
            Resolution = resolution;
            _points = points;
            _periods = periods;
            TaxIndicator = taxIndicator;
        }

        /// <summary>
        /// Minimal ctor to support EF Core.
        /// </summary>
        // ReSharper disable once UnusedMember.Local - used by EF Core
        private Charge()
        {
            SenderProvidedChargeId = null!;
            _points = new List<Point>();
            _periods = new List<ChargePeriod>();
        }

        /// <summary>
        /// Globally unique identifier of the charge.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Unique ID of a charge (Note, unique per market participants and charge type).
        /// Example: EA-001
        /// </summary>
        public string SenderProvidedChargeId { get; }

        public ChargeType Type { get; }

        /// <summary>
        ///  Aggregate ID reference to the owning market participant.
        /// </summary>
        public Guid OwnerId { get; }

        public Resolution Resolution { get; }

        /// <summary>
        /// Indicates whether the Charge is tax or not.
        /// </summary>
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local - private setter used in unit test
        public bool TaxIndicator { get; private set;  }

        public IReadOnlyCollection<Point> Points => _points;

        public IReadOnlyCollection<ChargePeriod> Periods => _periods;

        public static Charge Create(
            string operationId,
            string name,
            string description,
            string senderProvidedChargeId,
            Guid ownerId,
            ChargeType type,
            Resolution resolution,
            TaxIndicator taxIndicator,
            VatClassification vatClassification,
            bool transparentInvoicing,
            Instant startDate,
            Instant? stopDate = null)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(description);
            ArgumentNullException.ThrowIfNull(senderProvidedChargeId);

            var rules = new List<OperationValidationRuleContainer>
            {
                new(
                    new CreateChargeIsNotAllowedATerminationRuleDate(stopDate), operationId),
            };
            CheckRules(rules);

            var chargePeriod = ChargePeriod.Create(
                name,
                description,
                vatClassification,
                transparentInvoicing,
                startDate,
                InstantExtensions.GetEndDefault());

            return new Charge(
                Guid.NewGuid(),
                senderProvidedChargeId,
                ownerId,
                type,
                resolution,
                ParseTaxIndicator(taxIndicator),
                new List<Point>(),
                new List<ChargePeriod> { chargePeriod });
        }

        /// <summary>
        /// Use this method to update the charge periods timeline of a charge upon receiving a charge update request
        /// Please see the persist charge documentation where the update flow is covered:
        /// https://github.com/Energinet-DataHub/geh-charges/tree/main/docs/process-flows#persist-charge
        /// </summary>
        /// <param name="newChargePeriod">New Charge Period from update charge request</param>
        /// <param name="taxIndicator">Tax indicator in the update charge request</param>
        /// <param name="resolution">Resolution of the update charge request</param>
        /// <param name="operationId">Charge operation id</param>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="newChargePeriod"/> is empty</exception>
        public void Update(ChargePeriod newChargePeriod, TaxIndicator taxIndicator, Resolution resolution, string operationId)
        {
            ArgumentNullException.ThrowIfNull(newChargePeriod);
            ArgumentNullException.ThrowIfNull(operationId);

            // This should be in a separate rule and handled by ChargeOperationFailedException
            // in order to notify sender that operation failed because of this constraint
            if (newChargePeriod.EndDateTime != InstantExtensions.GetEndDefault())
            {
                throw new InvalidOperationException("Charge update must not have bound end date.");
            }

            var rules = GenerateRules(newChargePeriod, taxIndicator, resolution, operationId);
            CheckRules(rules);

            var stopDate = _periods.Max(p => p.EndDateTime);
            if (stopDate != InstantExtensions.GetEndDefault())
            {
                newChargePeriod = newChargePeriod.WithEndDate(stopDate);
            }

            if (_periods.Exists(p => p.StartDateTime < newChargePeriod.StartDateTime))
            {
                StopExistingPeriod(newChargePeriod.StartDateTime);
            }

            RemoveAllSubsequentPeriods(newChargePeriod.StartDateTime);
            _periods.Add(newChargePeriod);
        }

        /// <summary>
        /// Use this method to stop a charge upon receiving a stop charge request
        /// </summary>
        /// <param name="stopDate"></param>
        /// <exception cref="ArgumentNullException"><paramref name="stopDate"/> is <c>null</c></exception>
        public void Stop(Instant? stopDate)
        {
            if (stopDate == null)
            {
                throw new ArgumentNullException(nameof(stopDate));
            }

            StopExistingPeriod(stopDate.Value);
            RemoveAllSubsequentPeriods(stopDate.Value);
            _points.RemoveAll(p => p.Time >= stopDate);
        }

        public void CancelStop(ChargePeriod chargePeriod, TaxIndicator taxIndicator, Resolution resolution, string operationId)
        {
            ArgumentNullException.ThrowIfNull(chargePeriod);
            ArgumentNullException.ThrowIfNull(operationId);

            var existingLastPeriod = _periods.OrderByDescending(p => p.StartDateTime).First();

            // This should be in a separate rule and handled by ChargeOperationFailedException
            // in order to notify sender that operation failed because of this constraint
            if (chargePeriod.StartDateTime != existingLastPeriod.EndDateTime)
            {
                throw new InvalidOperationException(
                    "Cannot cancel stop when new start date is not equal to existing stop date.");
            }

            var rules = GenerateRules(chargePeriod, taxIndicator, resolution, operationId).ToList();
            CheckRules(rules);

            _periods.Add(chargePeriod);
        }

        public void UpdatePrices(Instant startDate, Instant endDate, IReadOnlyList<Point> newPrices, string operationId)
        {
            ArgumentNullException.ThrowIfNull(newPrices);
            ArgumentNullException.ThrowIfNull(operationId);

            if (newPrices.Count == 0) return;

            var rules = new List<OperationValidationRuleContainer>
            {
                new(
                    new UpdateChargeMustHaveStartDateBeforeOrOnStopDateRule(
                        endDate,
                        startDate),
                    operationId),
            };
            CheckRules(rules);

            RemoveExistingChargePrices(startDate, endDate);
            _points.AddRange(newPrices);
        }

        private static bool ParseTaxIndicator(TaxIndicator taxIndicator)
        {
            // TODO: TaxIndicator is refactored in upcoming PR and by then this temporary fix i going to be removed
            return taxIndicator switch
            {
                Charges.TaxIndicator.Tax => true,
                Charges.TaxIndicator.NoTax => false,
                _ => throw new InvalidOperationException("Tax indicator must be set."),
            };
        }

        private void RemoveExistingChargePrices(Instant? startDate, Instant? endDate)
        {
            if (startDate is null) return;
            if (endDate is null) return;
            bool Predicate(Point x) => x.Time >= startDate && x.Time < endDate;
            if (_points.Any(Predicate))
            {
                _points.RemoveAll(Predicate);
            }
        }

        private void StopExistingPeriod(Instant stopDate)
        {
            // When implementing this validation:
            // 'Cannot perform a stop charge when the charge is already stopped on the same date',
            // then p.EndDateTime = stopDate can be removed from conditions below, simplifying the code.
            var previousPeriod = _periods.FirstOrDefault(p =>
                    p.EndDateTime >= stopDate &&
                    p.StartDateTime <= stopDate);

            // This should be in a separate rule and handled by ChargeOperationFailedException
            // in order to notify sender that operation failed because of this constraint
            if (previousPeriod == null)
            {
                throw new InvalidOperationException("Cannot stop charge. No period exist on stop date.");
            }

            // Return if charge already has end date at given stop date
            if (stopDate == previousPeriod.EndDateTime) return;

            _periods.Remove(previousPeriod);

            if (stopDate == previousPeriod.StartDateTime) return;

            var newPreviousPeriod = previousPeriod.WithEndDate(stopDate);
            _periods.Add(newPreviousPeriod);
        }

        private void RemoveAllSubsequentPeriods(Instant date)
        {
            bool Predicate(ChargePeriod p) => p.StartDateTime >= date;
            if (_periods.Any(Predicate))
            {
                _periods.RemoveAll(Predicate);
            }
        }

        private IEnumerable<IValidationRuleContainer> GenerateRules(
            ChargePeriod newChargePeriod,
            TaxIndicator newTaxIndicator,
            Resolution newResolution,
            string operationId)
        {
            var rules = new List<IValidationRuleContainer>
            {
                new OperationValidationRuleContainer(
                    new ChangingTariffTaxValueNotAllowedRule(
                        newTaxIndicator,
                        TaxIndicator),
                    operationId),
                new OperationValidationRuleContainer(
                    new UpdateChargeMustHaveStartDateBeforeOrOnStopDateRule(
                        _periods.OrderBy(x => x.EndDateTime).Last().EndDateTime,
                        newChargePeriod.StartDateTime),
                    operationId),
                new OperationValidationRuleContainer(
                    new ChargeResolutionCanNotBeUpdatedRule(
                        Resolution,
                        newResolution),
                    operationId),
            };
            return rules;
        }

        private static void CheckRules(IEnumerable<IValidationRuleContainer> rules)
        {
            var invalidRules = rules.Where(r => !r.ValidationRule.IsValid).ToList();
            var result = invalidRules.Any()
                ? ValidationResult.CreateFailure(invalidRules)
                : ValidationResult.CreateSuccess();
            if (result.IsFailed)
            {
                throw new ChargeOperationFailedException(result.InvalidRules);
            }
        }
    }
}
