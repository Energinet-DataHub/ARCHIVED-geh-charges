using System;
using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;

namespace GreenEnergyHub.Charges.Tests.TestCore.ProtoBufAssertHelpers
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
