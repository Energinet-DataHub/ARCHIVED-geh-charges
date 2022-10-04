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

using System.Text.Json.Nodes;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim
{
    public static class CimJsonHelper
    {
        /// <summary>
        /// utility method for adding a descendant json construct with value and eventually a coding scheme.
        /// </summary>
        /// <param name="value">value to be added to the json construct</param>
        /// <param name="codingScheme">if a coding scheme must be explicitly specified</param>
        /// <returns>a json object with up to 2 element (value, codingScheme)</returns>
        public static JsonObject CreateValueObject(JsonNode? value, CodingScheme codingScheme = 0)
        {
            var obj = new JsonObject();
            if (codingScheme > 0)
            {
                obj.Add(CimMarketDocumentConstants.CodingScheme, CodingSchemeMapper.Map(codingScheme));
            }

            obj.Add("value", value);

            return obj;
        }
    }
}
