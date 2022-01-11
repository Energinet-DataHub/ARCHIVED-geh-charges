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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.Infrastructure.Core.Correlation;
using GreenEnergyHub.Charges.MessageHub.BundleSpecification;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.MessageHub
{
    public class AvailableDataNotifier<TAvailableData, TInputType> : IAvailableDataNotifier<TAvailableData, TInputType>
        where TAvailableData : AvailableDataBase
    {
        private readonly IAvailableDataFactory<TAvailableData, TInputType> _availableDataFactory;
        private readonly IAvailableDataRepository<TAvailableData> _availableDataRepository;
        private readonly IAvailableDataNotificationFactory<TAvailableData> _availableDataNotificationFactory;
        private readonly IDataAvailableNotificationSender _dataAvailableNotificationSender;
        private readonly ICorrelationContext _correlationContext;
        private readonly BundleSpecification<TAvailableData, TInputType> _bundleSpecification;

        public AvailableDataNotifier(
            IAvailableDataFactory<TAvailableData, TInputType> availableDataFactory,
            IAvailableDataRepository<TAvailableData> availableDataRepository,
            IAvailableDataNotificationFactory<TAvailableData> availableDataNotificationFactory,
            IDataAvailableNotificationSender dataAvailableNotificationSender,
            ICorrelationContext correlationContext,
            BundleSpecification<TAvailableData, TInputType> bundleSpecification)
        {
            _availableDataFactory = availableDataFactory;
            _availableDataRepository = availableDataRepository;
            _availableDataNotificationFactory = availableDataNotificationFactory;
            _dataAvailableNotificationSender = dataAvailableNotificationSender;
            _correlationContext = correlationContext;
            _bundleSpecification = bundleSpecification;
        }

        public async Task NotifyAsync(TInputType input)
        {
            var availableData = await CreateAvailableDataAsync(input);

            if (availableData.Count == 0)
                return;

            await StoreAvailableDataForLaterBundlingAsync(availableData).ConfigureAwait(false);

            await NotifyMessageHubOfAvailableDataAsync(availableData).ConfigureAwait(false);
        }

        private async Task<IReadOnlyList<TAvailableData>> CreateAvailableDataAsync(TInputType input)
        {
            return await _availableDataFactory.CreateAsync(input);
        }

        private async Task StoreAvailableDataForLaterBundlingAsync(IReadOnlyList<TAvailableData> availableData)
        {
            await _availableDataRepository.StoreAsync(availableData).ConfigureAwait(false);
        }

        private async Task NotifyMessageHubOfAvailableDataAsync(IReadOnlyList<TAvailableData> availableData)
        {
            var notifications = CreateNotifications(availableData);

            var dataAvailableNotificationSenderTasks = notifications
                .Select(notification => _dataAvailableNotificationSender.SendAsync(_correlationContext.Id, notification));

            await Task.WhenAll(dataAvailableNotificationSenderTasks).ConfigureAwait(false);
        }

        private IReadOnlyList<DataAvailableNotificationDto> CreateNotifications(
            IReadOnlyList<TAvailableData> availableData)
        {
            return _availableDataNotificationFactory.Create(availableData, _bundleSpecification);
        }
    }
}
