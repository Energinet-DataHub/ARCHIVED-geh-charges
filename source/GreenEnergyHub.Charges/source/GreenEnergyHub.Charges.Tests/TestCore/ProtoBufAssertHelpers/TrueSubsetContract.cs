using System;
using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;

namespace GreenEnergyHub.Charges.Tests.TestCore.ProtoBufAssertHelpers
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
