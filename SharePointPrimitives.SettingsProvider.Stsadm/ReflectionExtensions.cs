using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SharePointPrimitives.SettingsProvider.Stsadm {
    public static class ReflectionExtensions {
        public static IEnumerable<AttributeT> GetCustomAttributes<AttributeT>(this ICustomAttributeProvider provider, bool inherit) {
            return provider.GetCustomAttributes(typeof(AttributeT),inherit).OfType<AttributeT>();
        }

        public static AttributeT GetCustomAttribute<AttributeT>(this ICustomAttributeProvider provider, bool inherit) {
            return provider.GetCustomAttributes(typeof(AttributeT), inherit).OfType<AttributeT>().FirstOrDefault();
        }

        public static bool HasCustomAttribute<AttributeT>(this ICustomAttributeProvider provider, bool inherit) {
            return provider.GetCustomAttributes(typeof(AttributeT), inherit).OfType<AttributeT>().Any();

        }
    }
}
