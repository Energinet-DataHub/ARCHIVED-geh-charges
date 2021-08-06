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
using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;

namespace GreenEnergyHub.Charges.Tests.TestCore.ProtoBufAssertHelpers
{
    public class TestDomainTypeEquivalentContract : TestBaseContract, IMessage<TestDomainTypeEquivalentContract>
    {
        public string A { get; }

        public string B { get; }

        #region Irrelevant stuff

        public TestDomainTypeEquivalentContract(string a, string b)
            : base(null!)
        {
            A = a;
            B = b;
        }

        public bool Equals([AllowNull] TestDomainTypeEquivalentContract other)
        {
            return false;
        }

        public new TestDomainTypeEquivalentContract Clone()
        {
            throw new NotImplementedException();
        }

        public void MergeFrom(TestDomainTypeEquivalentContract message)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object? obj)
        {
            return ((IEquatable<TestDomainTypeEquivalentContract>)this).Equals(obj as TestDomainTypeEquivalentContract);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), A, B);
        }

        #endregion
    }
}
