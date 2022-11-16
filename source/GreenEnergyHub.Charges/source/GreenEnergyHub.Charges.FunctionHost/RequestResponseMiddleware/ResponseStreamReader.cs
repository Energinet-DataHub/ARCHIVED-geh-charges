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

using System.IO;
using System.Threading.Tasks;

namespace GreenEnergyHub.Charges.FunctionHost.RequestResponseMiddleware
{
    internal static class ResponseStreamReader
    {
        /// <summary>
        /// Returns copy of input stream. Expect stream to be seekable and position to be at 0;
        /// </summary>
        /// <returns>copy of stream</returns>
        public static async Task<Stream> CopyBodyStreamAsync(Stream fromStream)
        {
            var ms = new MemoryStream();
            await fromStream.CopyToAsync(ms).ConfigureAwait(false);
            await ms.FlushAsync().ConfigureAwait(false);

            ms.Position = 0;

            return ms;
        }
    }
}
