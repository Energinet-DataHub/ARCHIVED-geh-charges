namespace GreenEnergyHub.Charges.Tests.TestCore.ProtoBufAssertHelpers
{
    public class TestDomainType
    {
        public TestDomainType(string a, string b)
        {
            A = a;
            B = b;
        }

        public string A { get; }

        public string B { get; }
    }
}
