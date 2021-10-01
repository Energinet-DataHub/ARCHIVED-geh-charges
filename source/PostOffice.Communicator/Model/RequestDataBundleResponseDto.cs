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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GreenEnergyHub.PostOffice.Communicator.Model
{
    /// <summary>
    /// Contains the result of the request for bundle contents.
    /// </summary>
    public sealed class RequestDataBundleResponseDto
    {
        /// <summary>
        /// Creates a successful response to the bundle contents request.
        /// </summary>
        /// <param name="contentUri">The location of the bundle in Azure Blob Storage.</param>
        /// <param name="dataAvailableNotificationIds">A collection of guids identifying which data has been requested.</param>
        public RequestDataBundleResponseDto(Uri contentUri, IEnumerable<Guid> dataAvailableNotificationIds)
        {
            DataAvailableNotificationIds = dataAvailableNotificationIds;
            ContentUri = contentUri;
            IsErrorResponse = false;
        }

        /// <summary>
        /// Creates a failure response to the bundle contents request.
        /// </summary>
        /// <param name="responseError">The information about the error.</param>
        /// <param name="dataAvailableNotificationIds">A collection of guids identifying which data has been requested.</param>
        public RequestDataBundleResponseDto(DataBundleResponseErrorDto responseError, IEnumerable<Guid> dataAvailableNotificationIds)
        {
            DataAvailableNotificationIds = dataAvailableNotificationIds;
            ResponseError = responseError;
            IsErrorResponse = true;
        }

        /// <summary>
        /// Specifies whether the response has succeeded.
        /// If true, the ResponseError contains the information about the error.
        /// If false, the ContentUri points to a location of the bundle contents.
        /// </summary>
        [MemberNotNullWhen(false, nameof(ContentUri))]
        [MemberNotNullWhen(true, nameof(ResponseError))]
        public bool IsErrorResponse { get; }

        /// <summary>
        /// A collection of guids identifying which data has been requested.
        /// </summary>
        public IEnumerable<Guid> DataAvailableNotificationIds { get; }

        /// <summary>
        /// Points to a location of the bundle contents.
        /// Is null, if the request failed.
        /// </summary>
        public Uri? ContentUri { get; }

        /// <summary>
        /// Error information. Is null, if the request succeeded.
        /// </summary>
        public DataBundleResponseErrorDto? ResponseError { get; }
    }
}
