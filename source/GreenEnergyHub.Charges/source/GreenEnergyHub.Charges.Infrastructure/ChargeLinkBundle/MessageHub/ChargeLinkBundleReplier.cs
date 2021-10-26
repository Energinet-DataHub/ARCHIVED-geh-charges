﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.MessageHub.Client.Extensions;
using Energinet.DataHub.MessageHub.Client.Model;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Client.Storage;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub.Infrastructure;

namespace GreenEnergyHub.Charges.Infrastructure.ChargeLinkBundle.MessageHub
{
    public class ChargeLinkBundleReplier : IChargeLinkBundleReplier
    {
        private readonly IStorageHandler _storageHandler;
        private readonly IDataBundleResponseSender _dataBundleResponseSender;
        private readonly IMessageMetaDataContext _messageMetaDataContext;

        public ChargeLinkBundleReplier(
            IStorageHandler storageHandler,
            IDataBundleResponseSender dataBundleResponseSender,
            IMessageMetaDataContext messageMetaDataContext)
        {
            _storageHandler = storageHandler;
            _dataBundleResponseSender = dataBundleResponseSender;
            _messageMetaDataContext = messageMetaDataContext;
        }

        public async Task ReplyAsync(Stream bundleStream, DataBundleRequestDto request)
        {
            var path = await _storageHandler.AddStreamToStorageAsync(bundleStream, request);

            var response = request.CreateResponse(path);

            await _dataBundleResponseSender
                .SendAsync(response, request, _messageMetaDataContext.SessionId)
                .ConfigureAwait(false);
        }
    }
}
