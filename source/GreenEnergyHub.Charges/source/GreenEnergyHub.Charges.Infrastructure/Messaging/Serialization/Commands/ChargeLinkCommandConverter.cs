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

using System.Threading.Tasks;
using System.Xml;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization.MarketDocument;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization.Commands
{
    public class ChargeLinkCommandConverter : DocumentConverter
    {
        private readonly ICorrelationContext _correlationContext;

        public ChargeLinkCommandConverter(
            ICorrelationContext correlationContext)
        {
            _correlationContext = correlationContext;
        }

        protected override async Task<IInboundMessage> ConvertSpecializedContentAsync(XmlReader reader, Document document)
        {
            var correlationId = _correlationContext.CorrelationId;

            var result = new ChargeLinkCommand(correlationId)
            {
                Document = document,
            };

            return await Task.FromResult(result).ConfigureAwait(false);
        }
    }
}
