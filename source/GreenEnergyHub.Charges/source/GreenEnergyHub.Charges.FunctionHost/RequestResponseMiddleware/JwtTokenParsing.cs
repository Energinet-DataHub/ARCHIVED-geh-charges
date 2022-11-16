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

using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.RequestResponseMiddleware
{
    internal static class JwtTokenParsing
    {
        private static readonly JwtSecurityTokenHandler _tokenHandler = new();

        public static string ReadJwtActorId(FunctionContext context)
        {
            try
            {
                if (context.BindingContext.BindingData.TryGetValue("headers", out var headerParams) && headerParams is string headerParamsString)
                {
                    var headerMatch = Regex.Match(headerParamsString, "\"[aA]uthorization\"\\s*:\\s*\"Bearer (.*?)\"");
                    if (headerMatch.Success && headerMatch.Groups.Count == 2)
                    {
                        var token = headerMatch.Groups[1].Value;
                        var parsed = _tokenHandler.ReadJwtToken(token);

                        if (parsed is null)
                        {
                            return string.Empty;
                        }

                        return parsed.Payload.Azp ?? string.Empty;
                    }
                }
            }
            catch
            {
                // ignored
            }

            return string.Empty;
        }
    }
}
