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
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub.Infrastructure;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub
{
    public class ChargeLinkBundleSender : IChargeLinkBundleSender
    {
        private readonly IChargeLinkBundleCreator _chargeLinkBundleCreator;
        private readonly IChargeLinkBundleReplier _chargeLinkBundleReplier;

        public ChargeLinkBundleSender(
            IChargeLinkBundleCreator chargeLinkBundleCreator,
            IChargeLinkBundleReplier chargeLinkBundleReplier)
        {
            _chargeLinkBundleCreator = chargeLinkBundleCreator;
            _chargeLinkBundleReplier = chargeLinkBundleReplier;
        }

        public async Task SendAsync(DataBundleRequestDto request)
        {
            await using var bundleStream = new MemoryStream();
            await _chargeLinkBundleCreator.CreateAsync(request, bundleStream);
            await _chargeLinkBundleReplier.ReplyAsync(bundleStream, request);
        }
    }
}
