using System;

namespace GreenEnergyHub.Charges.Core.Enumeration
{
    public static class EnumExtensions
    {
        public static T Parse<T>(this Enum source, string enumValue)
            where T : struct, Enum, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

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
