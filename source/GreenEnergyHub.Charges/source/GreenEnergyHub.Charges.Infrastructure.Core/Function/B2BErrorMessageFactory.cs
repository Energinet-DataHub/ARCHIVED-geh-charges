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

using System.ComponentModel;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Function
{
    public static class B2BErrorMessageFactory
    {
        public static B2BErrorMessage Create(B2BErrorCode code)
        {
            return code switch
            {
                B2BErrorCode.ActorIsNotWhoTheyClaimToBeErrorMessage =>
                    new B2BErrorMessage(
                        "B2B-008",
                        "The sender organization provided in the request body does not match the organization in the bearer token."),

                _ =>
                    throw new InvalidEnumArgumentException($"Provided B2B error code '{code}' is invalid and cannot be mapped."),
            };
        }
    }
}
