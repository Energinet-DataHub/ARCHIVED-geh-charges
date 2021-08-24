using System;

namespace GreenEnergyHub.Charges.Core.Enumeration
{
    public class EnumCasting
    {
        public static T GetEnumFromString<T>(string enumValue)
        where T : struct, Enum
        {
            var enumConversionSuccess = Enum.TryParse<T>(
                enumValue,
                out var convertedEnum);

            if (enumConversionSuccess is false)
            {
                throw new ArgumentException($"Enum converting value: {enumValue} to type {typeof(T).FullName}");
            }

            return convertedEnum;
        }
    }
}
