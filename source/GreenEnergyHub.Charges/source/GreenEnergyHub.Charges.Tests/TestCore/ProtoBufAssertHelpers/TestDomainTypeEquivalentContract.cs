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
