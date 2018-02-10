using System;

namespace Demo.ScatterGather.Core
{
    public class Guard
    {
        public static void AgainstNull(object value, string name = null)
        {
            if (value == null)
            {
                throw name == null ? new ArgumentNullException() : new ArgumentNullException(name);
            }
        }
    }
}
