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

using System.Collections.Generic;

namespace GreenEnergyHub.Charges.FunctionHost.RequestResponseMiddleware.Models
{
    internal static class IndexTagsKeys
    {
        public const string JwtActorId = "JwtActorId";
        public const string FunctionId = "FunctionId";
        public const string FunctionName = "FunctionName";
        public const string InvocationId = "InvocationId";
        public const string TraceParent = "TraceParent";
        public const string TraceId = "TraceId";
        public const string HttpDataType = "HttpDataType";
        public const string StatusCode = "StatusCode";
        public const string CorrelationId = "CorrelationId";
        public const string UniqueLogName = "UniqueLogName";
        public const string MessageId = "MessageId";
        public const string MessageType = "MessageType";

        public static IEnumerable<string> GetKeys()
        {
            return new List<string>()
            {
                JwtActorId,
                FunctionId,
                FunctionName,
                InvocationId,
                TraceParent,
                TraceId,
                HttpDataType,
                StatusCode,
                CorrelationId,
                UniqueLogName,
                MessageId,
                MessageType,
            };
        }

        public static List<string> GetKeysForMax10Items()
        {
            return new List<string>()
            {
                JwtActorId,
                FunctionName,
                InvocationId,
                TraceId,
                HttpDataType,
                StatusCode,
                CorrelationId,
            };
        }
    }
}
