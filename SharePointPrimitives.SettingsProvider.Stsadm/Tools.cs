using System;
using System.Reflection;
using System.Configuration;
using System.Linq;

namespace SharePointPrimitives.SettingsProvider.Stsadm {
    public static class Tools {
        public static Assembly LoadAssembly(string name, string path) {
            try {
                if (!String.IsNullOrEmpty(name)) {
                    return Assembly.LoadWithPartialName(name); // I am aware it is depercated but it is soo useful
                } else {
                    return Assembly.LoadFile(path);
                }
            } catch {
                return null;
            }
        }

        static private readonly Type ApplicationSettingsBaseT = typeof(ApplicationSettingsBase);
        static private readonly Type providerT = typeof(Provider);

        public static bool HasSettings(Assembly assembly) {
            return assembly.GetTypes().Any(t => t.BaseType == ApplicationSettingsBaseT);
        }

        public static Type GetSettings(Assembly assembly) {
            Type settingsT =  assembly.GetTypes().FirstOrDefault(t => t.BaseType == ApplicationSettingsBaseT);

            var attr = settingsT.GetCustomAttribute<SettingsProviderAttribute>(true);
            if (attr == null || !attr.ProviderTypeName.StartsWith(providerT.FullName)) {
                return null;
            }
            return settingsT;
        }
    }
}
