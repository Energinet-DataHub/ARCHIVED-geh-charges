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

using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Protobuf;
using GreenEnergyHub.Charges.Tests.TestCore.Protobuf.ProtobufAssertHelpers;
using Xunit;
using Xunit.Categories;
using Xunit.Sdk;

namespace GreenEnergyHub.Charges.Tests.TestCore.Protobuf
{
    [UnitTest]
    public class ProtobufAssertTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void OutgoingContractIsSubset_Fails_WhenContractIsTrueSuperset(TestDomainType domain, TrueSupersetContract trueTrueSupersetContract)
        {
            Assert.Throws<XunitException>(
                () => ProtobufAssert.OutgoingContractIsSubset<TrueSupersetContract>(domain, trueTrueSupersetContract));
        }

        [Theory]
        [InlineAutoMoqData]
        public void OutgoingContractIsSubset_Succeeds_WhenContractIsTrueSubset([NotNull]TestDomainType domain)
        {
            var trueSubsetContract = new TrueSubsetContract(domain.A);
            ProtobufAssert.OutgoingContractIsSubset<TrueSubsetContract>(domain, trueSubsetContract);
        }

        [Theory]
        [InlineAutoMoqData]
        public void OutgoingContractIsSubset_Succeeds_WhenContractIsEquivalent([NotNull]TestDomainType domain)
        {
            var subsetContract = new TestDomainTypeEquivalentContract(domain.A, domain.B);
            ProtobufAssert.OutgoingContractIsSubset<TestDomainTypeEquivalentContract>(domain, subsetContract);
        }

        [Theory]
        [InlineAutoMoqData]
        public void IncomingContractIsSuperset_Fails_WhenContractIsTrueSubset([NotNull]TestDomainType domain)
        {
            var trueSubsetContract = new TrueSubsetContract(domain.A);
            Assert.Throws<XunitException>(
                () => ProtobufAssert.IncomingContractIsSuperset<TrueSubsetContract>(domain, trueSubsetContract));
        }

        [Theory]
        [InlineAutoMoqData]
        public void IncomingContractIsSuperset_Succeeds_WhenContractIsEquivalent([NotNull]TestDomainType domain)
        {
            var equivalentContract = new TestDomainTypeEquivalentContract(domain.A, domain.B);
            ProtobufAssert.IncomingContractIsSuperset<TestDomainTypeEquivalentContract>(domain, equivalentContract);
        }

        [Theory]
        [InlineAutoMoqData]
        public void IncomingContractIsSuperset_Succeeds_WhenContractIsTrueSuperset([NotNull]TrueSupersetContract trueSupersetContract)
        {
            var domain = new TestDomainType(trueSupersetContract.A, trueSupersetContract.B);
            ProtobufAssert.IncomingContractIsSuperset<TrueSupersetContract>(domain, trueSupersetContract);
        }
    }
}
