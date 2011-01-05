using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharePointPrimitives.SettingsProvider.Stsadm {
    public static class ReflectionExtensions {
        public static IEnumerable<AttributeT> GetCustomAttributes<AttributeT>(this Type type, bool inherit) {
            return type.GetCustomAttributes(typeof(AttributeT),inherit).OfType<AttributeT>();
        }
    }
}
