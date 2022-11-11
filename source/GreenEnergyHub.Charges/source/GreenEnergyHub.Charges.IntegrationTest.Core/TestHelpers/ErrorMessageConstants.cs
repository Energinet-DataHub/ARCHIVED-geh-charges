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

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers
{
    public static class ErrorMessageConstants
    {
        public const string ActorIsNotWhoTheyClaimToBeErrorMessage =
            "The sender organization provided in the request body does not match the organization in the bearer token.";

        public const string SyntaxValidationErrorMessage =
            "Syntax validation failed for business message.";

        public const string AnElementContainsAnInvalidValue =
            "{0} element contains an invalid value. ";

        public const string ValueIsEmptyOrIsWhiteSpace =
            "It is either empty or contains only whitespace.";

        public const string UnsupportedResolutionErrorMessage =
            "Provided Resolution value '{0}' is invalid and cannot be mapped.";

        public const string UnsupportedBusinessReasonCodeErrorMessage =
            "Provided BusinessReasonCode value '{0}' is invalid and cannot be mapped.";
    }
}
