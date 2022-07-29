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

using System.ComponentModel;
using GreenEnergyHub.Charges.Domain.AvailableData.Shared;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument
{
    public static class ReceiptStatusMapper
    {
        private const string CimConfirmed = "A01"; // Temporary value extracted from example file until real are known
        private const string CimRejected = "A02"; // Temporary value extracted from example file until real are known

        public static string Map(ReceiptStatus receiptStatus)
        {
            return receiptStatus switch
            {
                ReceiptStatus.Confirmed => CimConfirmed,
                ReceiptStatus.Rejected => CimRejected,
                _ => throw new InvalidEnumArgumentException(
                    $"Provided ReceiptStatus value '{receiptStatus}' is invalid and cannot be mapped."),
            };
        }
    }
}
