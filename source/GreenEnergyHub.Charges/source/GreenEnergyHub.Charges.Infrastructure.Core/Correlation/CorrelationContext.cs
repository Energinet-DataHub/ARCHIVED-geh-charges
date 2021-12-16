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

namespace GreenEnergyHub.Charges.Infrastructure.Core.Correlation
{
    public class CorrelationContext : ICorrelationContext
    {
        private string? _id;

        private string? _parentId;

        public string Id => _id ?? throw new InvalidOperationException("Correlation id not set");

        public string? ParentId => _parentId;

        public void SetId(string id)
        {
            _id = id;
        }

        public void SetParentId(string parentId)
        {
            _parentId = parentId;
        }

        public string AsTraceContext()
        {
            if (string.IsNullOrEmpty(_id) || string.IsNullOrEmpty(_parentId))
            {
                return string.Empty;
            }

            return $"00-{_id}-{_parentId}-00"; // Specification of format can be found here: https://www.w3.org/TR/trace-context/#trace-context-http-headers-format
        }
    }
}
