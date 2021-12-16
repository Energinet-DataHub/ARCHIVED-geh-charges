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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace GreenEnergyHub.Charges.TestCore
{
    public static class EmbeddedStreamHelper
    {
        public static Stream GetInputStream([NotNull] Assembly assembly, string streamPath)
        {
            var result = assembly.GetManifestResourceStream(streamPath);
            if (result == null)
            {
                throw new NotImplementedException($"The filename {streamPath} has not been added as an embedded resource to the project");
            }

            return result;
        }

        public static string GetEmbeddedStreamAsString(Assembly assembly, string streamPath)
        {
            using var stream = GetInputStream(assembly, streamPath);
            return stream.AsString();
        }
    }
}
