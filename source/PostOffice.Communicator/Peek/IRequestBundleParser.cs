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

using GreenEnergyHub.PostOffice.Communicator.Exceptions;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Peek
{
    /// <summary>
    /// Parses the bundle content request sent to a sub-domain.
    /// </summary>
    public interface IRequestBundleParser
    {
        /// <summary>
        /// Converts the specified request into a protobuf contract.
        /// </summary>
        /// <param name="request">The request to convert.</param>
        /// <returns>A byte array with the parsed DataBundleRequestDto</returns>
        public byte[] Parse(DataBundleRequestDto request);

        /// <summary>
        /// Parses the protobuf contract request.
        /// </summary>
        /// <param name="dataBundleRequestContract">The bytes containing the protobuf contract.</param>
        /// <returns><see cref="DataBundleRequestDto"/></returns>
        /// <exception cref="PostOfficeCommunicatorException">
        /// Throws an exception if byte array cannot be parsed.
        /// </exception>
        DataBundleRequestDto Parse(byte[] dataBundleRequestContract);
    }
}
