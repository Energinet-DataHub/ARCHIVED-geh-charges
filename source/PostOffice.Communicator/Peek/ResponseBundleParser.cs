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
using System.Linq;
using Google.Protobuf;
using GreenEnergyHub.PostOffice.Communicator.Contracts;
using GreenEnergyHub.PostOffice.Communicator.Exceptions;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Peek
{
    public class ResponseBundleParser : IResponseBundleParser
    {
        public byte[] Parse(RequestDataBundleResponseDto requestDataBundleResponseDto)
        {
            if (requestDataBundleResponseDto == null)
                throw new ArgumentNullException(nameof(requestDataBundleResponseDto));
            var contract = new RequestBundleResponse();

            if (!requestDataBundleResponseDto.IsErrorResponse)
            {
                contract.Success = new RequestBundleResponse.Types.FileResource { Uri = requestDataBundleResponseDto.ContentUri?.AbsoluteUri };
                return contract.ToByteArray();
            }

            var contractErrorReason = MapToFailureReason(requestDataBundleResponseDto.ResponseError!.Reason);
            contract.Failure = new RequestBundleResponse.Types.RequestFailure { Reason = contractErrorReason, FailureDescription = requestDataBundleResponseDto.ResponseError.FailureDescription };
            return contract.ToByteArray();
        }

        public RequestDataBundleResponseDto? Parse(byte[] dataBundleReplyContract)
        {
            try
            {
                var bundleResponse = RequestBundleResponse.Parser.ParseFrom(dataBundleReplyContract);
                return bundleResponse!.ReplyCase != RequestBundleResponse.ReplyOneofCase.Success
                    ? null
                    : new RequestDataBundleResponseDto(new Uri(bundleResponse.Success.Uri), bundleResponse.Success.UUID.Select(Guid.Parse).ToList());
            }
            catch (InvalidProtocolBufferException e)
            {
                throw new PostOfficeCommunicatorException("Error parsing bytes for DataBundleRequestDto", e);
            }
        }

        private static RequestBundleResponse.Types.RequestFailure.Types.Reason MapToFailureReason(DataBundleResponseErrorReason errorReason)
        {
            return errorReason switch
            {
                DataBundleResponseErrorReason.DatasetNotFound => RequestBundleResponse.Types.RequestFailure.Types.Reason.DatasetNotFound,
                DataBundleResponseErrorReason.DatasetNotAvailable => RequestBundleResponse.Types.RequestFailure.Types.Reason.DatasetNotAvailable,
                DataBundleResponseErrorReason.InternalError => RequestBundleResponse.Types.RequestFailure.Types.Reason.InternalError,
                _ => RequestBundleResponse.Types.RequestFailure.Types.Reason.InternalError
            };
        }
    }
}
