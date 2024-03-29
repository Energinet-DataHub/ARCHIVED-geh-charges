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

using System;
using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;

namespace GreenEnergyHub.Charges.Tests.TestCore.Protobuf.ProtobufAssertHelpers
{
    /// <summary>
    /// Contract that is lacking prop <see cref="TestDomainType.B"/>.
    /// </summary>
    public class TrueSubsetContract : TestBaseContract, IMessage<TrueSubsetContract>
    {
        public string A { get; }

        #region Irrelevant stuff

        public TrueSubsetContract(string a)
            : base(null!)
        {
            A = a;
        }

        public bool Equals([AllowNull] TrueSubsetContract other)
        {
            return false;
        }

        public new TrueSubsetContract Clone()
        {
            throw new NotImplementedException();
        }

        public void MergeFrom(TrueSubsetContract message)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object? obj)
        {
            return ((IEquatable<TrueSubsetContract>)this).Equals(obj as TrueSubsetContract);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), A);
        }

        #endregion
    }
}
