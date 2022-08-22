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
using GreenEnergyHub.Charges.Infrastructure.Core.Cim;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.TestCore.TestHelpers;

namespace GreenEnergyHub.Charges.Tests.MessageHub.BundleSpecification
{
    public static class AvailableReceiptValidationErrorGenerator
    {
        private const int MaxTextLengthInTest = 10000;

        public static List<AvailableReceiptValidationError> CreateReasons(int noOfReasons, ReasonCode reasonCode = ReasonCode.D01)
        {
            var reasons = new List<AvailableReceiptValidationError>();

            for (var i = 0; i < noOfReasons; i++)
            {
                reasons.Add(CreateReason(reasonCode));
            }

            return reasons;
        }

        private static AvailableReceiptValidationError CreateReason(ReasonCode reasonCode)
        {
            var text = StringGenerator.CreateStringOfRandomLength(MaxTextLengthInTest);
            return new AvailableReceiptValidationError(reasonCode, text);
        }
    }
}
