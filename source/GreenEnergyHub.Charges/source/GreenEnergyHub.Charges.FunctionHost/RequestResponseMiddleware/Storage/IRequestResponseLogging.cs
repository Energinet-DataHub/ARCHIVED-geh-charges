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
using System.IO;
using System.Threading.Tasks;

namespace GreenEnergyHub.Charges.FunctionHost.RequestResponseMiddleware.Storage
{
    /// <summary>
    /// Interface for request and response logging middleware.
    /// </summary>
    public interface IRequestResponseLogging
    {
        /// <summary>
        /// Logs request
        /// </summary>
        /// <param name="logStream">stream to log</param>
        /// <param name="metaData">log metaData</param>
        /// <param name="indexTags">index tags</param>
        /// <param name="logName"></param>
        /// <returns>work task</returns>
        Task LogRequestAsync(Stream logStream, Dictionary<string, string> metaData, Dictionary<string, string> indexTags, string logName);

        /// <summary>
        /// Logs response
        /// </summary>
        /// <param name="logStream">stream to log</param>
        /// <param name="metaData">log metaData</param>
        /// <param name="indexTags">index tags</param>
        /// <param name="logName"></param>
        /// <returns>Work task</returns>
        Task LogResponseAsync(Stream logStream, Dictionary<string, string> metaData, Dictionary<string, string> indexTags, string logName);
    }
}
