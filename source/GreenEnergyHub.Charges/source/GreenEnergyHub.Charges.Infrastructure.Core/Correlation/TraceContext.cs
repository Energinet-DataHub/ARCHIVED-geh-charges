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

using System.Diagnostics.CodeAnalysis;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Correlation
{
    /// <summary>
    /// Implementation of w3c trace context
    /// </summary>
    /// <remarks>
    /// For now, the implementation doesn't handle all validation nor uses version or flags.
    /// Specification can be found here: https://www.w3.org/TR/trace-context/#trace-id
    /// </remarks>
    public class TraceContext
    {
        // TODO: Use ActivityContext.Parse()?
        private TraceContext(string traceId, string parentId, bool isValid)
        {
            TraceId = traceId;
            ParentId = parentId;
            IsValid = isValid;
        }

        public string TraceId { get; }

        public string ParentId { get; }

        public bool IsValid { get; }

        public static TraceContext Parse([NotNull] string traceContext)
        {
            if (string.IsNullOrWhiteSpace(traceContext)) return Invalid();
            if (traceContext.Length != 55) return Invalid();

            var parts = traceContext.Split('-');
            if (parts.Length != 4) return Invalid();

            var traceId = parts[1];
            var parentId = parts[2];
            if (traceId.Length != 32 || parentId.Length != 16) return Invalid();

            return Create(traceId, parentId);
        }

        private static TraceContext Create(string traceId, string parentId)
        {
            return new TraceContext(
                traceId,
                parentId,
                true);
        }

        private static TraceContext Invalid()
        {
            return new TraceContext(
                string.Empty,
                string.Empty,
                false);
        }
    }
}
