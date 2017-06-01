using System;

namespace Autofac.Configuration.Util
{
    internal static class Enforce
    {
        public static T ArgumentNotNull<T>(T value, string name) where T : class
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
            return value;
        }
    }
}