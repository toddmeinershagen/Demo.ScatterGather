using System.ComponentModel;

namespace Demo.ScatterGather.Core
{
    public static class StringExtensions
    {
        public static T? Parse<T>(this string value) where T : struct
        {
            try
            {
                if (string.IsNullOrEmpty(value?.Trim()))
                {
                    return null;
                }

                return (T)TypeDescriptor
                    .GetConverter(typeof(T))
                    .ConvertFromInvariantString(value);
            }
            catch
            {
                return null;
            }
        }
    }
}
