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

namespace GreenEnergyHub.Charges.Domain.MeteringPoint
{
    public enum MeteringPointType
    {
        Unknown = 0,
        Consumption = 1, // This will be received in E17 in ebiX
        Production = 2, // This will be received in E18 in ebiX
        Exchange = 3, // This will be received in E20 in ebiX
        VeProduction = 4, // This will be received as D01 in ebiX
        Analysis = 5, // This will be received as D02 in ebiX
        SurplusProductionGroup = 6, // This will be received as D04 in ebiX
        NetProduction = 7, // This will be received as D05 in ebiX
        SupplyToGrid = 8, // This will be received as D06 in ebiX
        ConsumptionFromGrid = 9, // This will be received as D07 in ebiX
        WholesaleService = 10, // This will received as D08 in ebiX
        OwnProduction = 11, // This will be received as D09 i ebiX
        NetFromGrid = 12, // This will be received as D10 in ebiX
        NetToGrid = 13, // This will be received as D11 in ebiX
        TotalConsumption = 14, // This will be received as D12 in ebiX
        GridLossCorrection = 15, // This will be received as D13 in ebiX
        ElectricalHeating = 16, // This will be received as D14 in ebiX
        NetConsumption = 17, // This will be received as D15 in ebiX
        OtherConsumption = 18, // This will be received as D17 in ebiX
        OtherProduction = 19, // This will be received as D18 in ebiX
        ExchangeReactiveEnergy = 20, // This will be received as D20 in ebiX
        InternalUse = 21, // This will be received as D99 in ebiX
    }
}
