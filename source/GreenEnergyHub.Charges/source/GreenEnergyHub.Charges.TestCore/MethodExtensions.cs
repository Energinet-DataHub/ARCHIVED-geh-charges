using System;
using System.Runtime.CompilerServices;

namespace GreenEnergyHub.Charges.TestCore
{
    public static class MethodExtensions
    {
        public static string GetMethodName(this object type, [CallerMemberName] string caller = null!)
        {
            return type == null
                ? throw new ArgumentNullException(nameof(type))
                : type.GetType().Name + "." + caller;
        }
    }
}
