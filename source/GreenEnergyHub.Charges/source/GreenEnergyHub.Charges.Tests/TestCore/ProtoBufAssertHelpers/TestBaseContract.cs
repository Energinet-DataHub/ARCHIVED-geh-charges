using System;
using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace GreenEnergyHub.Charges.Tests.TestCore.ProtoBufAssertHelpers
{
    public abstract class TestBaseContract : IMessage<TestBaseContract>
    {
        public TestBaseContract(MessageDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        public void MergeFrom(TestBaseContract message)
        {
            throw new NotImplementedException();
        }

        public void MergeFrom(CodedInputStream input)
        {
            throw new NotImplementedException();
        }

        public void WriteTo(CodedOutputStream output)
        {
            throw new NotImplementedException();
        }

        public int CalculateSize()
        {
            throw new NotImplementedException();
        }

        public MessageDescriptor Descriptor { get; }

        public bool Equals([AllowNull] TestBaseContract other)
        {
            return false;
        }

        public TestBaseContract Clone()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TestBaseContract);
        }

        public override int GetHashCode()
        {
            return Descriptor.GetHashCode();
        }
    }
}
