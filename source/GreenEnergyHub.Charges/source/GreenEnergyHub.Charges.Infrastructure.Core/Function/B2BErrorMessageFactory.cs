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

namespace GreenEnergyHub.Charges.Infrastructure.Core.Function
{
    public static class B2BErrorMessageFactory
    {
        public static B2BErrorMessage CreateSenderNotAuthorizedErrorMessage()
        {
            return new B2BErrorMessage(
                B2BErrorCodeConstants.SenderIsNotAuthorized,
                "The sender organization provided in the request body does not match the organization in the bearer token.");
        }

        public static B2BErrorMessage CreateIsEmptyOrWhitespaceErrorMessage(string errorIdentifier)
        {
            return new B2BErrorMessage(
                B2BErrorCodeConstants.SyntaxValidation,
                $"Syntax validation failed for business message.{Environment.NewLine}'{errorIdentifier}' is either empty or contains only whitespace.");
        }

        public static B2BErrorMessage CreateCouldNotMapEnumErrorMessage(string errorIdentifier, string errorContent)
        {
            return new B2BErrorMessage(
                B2BErrorCodeConstants.SyntaxValidation,
                $"Syntax validation failed for business message.{Environment.NewLine}Provided '{errorIdentifier}' value '{errorContent}' is invalid.");
        }
    }
}
