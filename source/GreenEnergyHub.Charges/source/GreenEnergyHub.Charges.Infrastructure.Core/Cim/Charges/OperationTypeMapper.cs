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
using GreenEnergyHub.Charges.Domain.Charges;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges
{
    public static class OperationTypeMapper
    {
        private const string CimCreate = "X01"; // TODO these are not decided constants, they are here to show the concept
        private const string CimUpdate = "X02";
        private const string CimStop = "X03";
        private const string CimCancelStop = "X04";

        public static OperationType Map(string value)
        {
            return value switch
            {
                CimCreate => OperationType.Create,
                CimUpdate => OperationType.Update,
                CimStop => OperationType.Stop,
                CimCancelStop => OperationType.CancelStop,
                _ => OperationType.Unknown,
            };
        }

        public static string Map(OperationType operationType)
        {
            return operationType switch
            {
                OperationType.Create => CimCreate,
                OperationType.Update => CimUpdate,
                OperationType.Stop => CimStop,
                OperationType.CancelStop => CimCancelStop,
                _ => throw new InvalidEnumArgumentException(
                    $"Provided OperationType value '{operationType}' is invalid and cannot be mapped."),
            };
        }
    }
}
