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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;

namespace GreenEnergyHub.Charges.MessageHub.Models.Shared
{
    public static class CimValidationErrorTextTokenMatcher
    {
        public static IEnumerable<CimValidationErrorTextToken> GetTokens(string errorTextTemplate)
        {
            // regex to match content between {{ and }} inspired by https://stackoverflow.com/a/16538131
            var matchList = Regex.Matches(errorTextTemplate, @"(?<=\{{)[^}]*(?=\}})");
            return matchList.Select(match =>
                (CimValidationErrorTextToken)Enum.Parse(typeof(CimValidationErrorTextToken), match.Value)).ToList();
        }
    }
}
