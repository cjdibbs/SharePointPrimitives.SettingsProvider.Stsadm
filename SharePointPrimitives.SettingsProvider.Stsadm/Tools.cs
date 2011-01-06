#region BSD license
// Copyright 2010 Chris Dibbs. All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
//
//   1. Redistributions of source code must retain the above copyright notice, this list of
//      conditions and the following disclaimer.
//
//   2. Redistributions in binary form must reproduce the above copyright notice, this list
//      of conditions and the following disclaimer in the documentation and/or other materials
//      provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY Chris Dibbs ``AS IS'' AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Chris Dibbs OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
// ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// The views and conclusions contained in the software and documentation are those of the
// authors and should not be interpreted as representing official policies, either expressed
// or implied, of Chris Dibbs.
#endregion
using System;
using System.Reflection;
using System.Configuration;
using System.Linq;

namespace SharePointPrimitives.SettingsProvider.Stsadm {
    public static class Tools {
        public static Assembly LoadAssembly(string name, string path) {
            try {
                if (!String.IsNullOrEmpty(name)) {
#pragma warning disable 0618  // I am aware it is depercated but it is soo useful
                    return Assembly.LoadWithPartialName(name);
#pragma warning restore 0618
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
