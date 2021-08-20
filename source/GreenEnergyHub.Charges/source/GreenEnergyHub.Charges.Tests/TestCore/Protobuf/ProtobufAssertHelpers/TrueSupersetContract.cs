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

namespace GreenEnergyHub.Charges.Tests.TestCore.Protobuf.ProtobufAssertHelpers
{
    public class TrueSupersetContract : TestBaseContract, IMessage<TrueSupersetContract>
    {
        public string A { get; }

        public string B { get; }

        /// <summary>
        /// Property that does not exist in <see cref="TestDomainType"/>.
        /// </summary>
        public string C { get; }

        #region Irrelevant stuff

        public TrueSupersetContract(string a, string b, string c)
            : base(null!)
        {
            A = a;
            B = b;
            C = c;
        }

        bool IEquatable<TrueSupersetContract>.Equals([AllowNull] TrueSupersetContract other)
        {
            return false;
        }

        public new TrueSupersetContract Clone()
        {
            throw new NotImplementedException();
        }

        public void MergeFrom(TrueSupersetContract message)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object? obj)
        {
            return ((IEquatable<TrueSupersetContract>)this).Equals(obj as TrueSupersetContract);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), A, B, C);
        }

        #endregion
    }
}
