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

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim
{
    public class CimIdProvider : ICimIdProvider
    {
        public string GetUniqueId()
        {
            /* A GUID ensure our uniqueness, and to comply with an ID that will not
               give conversions to eBIX an issue, we limit it to below 35 characters
               by stripping the '-' character */
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }
}
