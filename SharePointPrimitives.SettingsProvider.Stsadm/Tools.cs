using System;
using System.Reflection;

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
    }
}
