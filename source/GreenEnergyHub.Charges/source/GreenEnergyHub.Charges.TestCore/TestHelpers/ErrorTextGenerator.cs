﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Text;

namespace GreenEnergyHub.Charges.TestCore.TestHelpers
{
    public static class ErrorTextGenerator
    {
        public static string CreateExpectedErrorMessage(
            string documentId,
            string documentType,
            string gln,
            string validationRuleIdentifier,
            int numberOfSubsequentErrors)
        {
            var expectedMessageBuilder = new StringBuilder();

            expectedMessageBuilder.AppendLine($"ValidationErrors for document Id {documentId} with Type {documentType} from GLN {gln}:");
            expectedMessageBuilder.AppendLine($"- ValidationRuleIdentifier: {validationRuleIdentifier}");

            for (var i = 0; i < numberOfSubsequentErrors; i++)
            {
                expectedMessageBuilder.AppendLine("- ValidationRuleIdentifier: SubsequentBundleOperationsFail");
            }

            return expectedMessageBuilder.ToString();
        }
    }
}
