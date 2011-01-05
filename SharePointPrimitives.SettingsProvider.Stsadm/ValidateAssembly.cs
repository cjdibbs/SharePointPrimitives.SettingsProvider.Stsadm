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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using SharePointPrimitives.Stsadm;
using System.Reflection;
using System.Configuration;
using Provider = SharePointPrimitives.SettingsProvider;

namespace SharePointPrimitives.SettingsProvider.Stsadm {
    public class ValidateAssembly : BaseCommand {

        string name;
        string path;

        protected override string HelpDescription {
            get { return "Validates that the settings for the assembly are in the database"; }
        }

        protected override IEnumerable<CommandArgument> CommandArguments {
            get {
                yield return new CommandArgument() {
                    Name = "name",
                    ArgumentRequired = true,
                    CommandRequired = false,
                    Help = "Full or Partal name of the assembly to load",
                    OnCommand = s => name = s
                };
                yield return new CommandArgument() {
                    Name = "path",
                    ArgumentRequired = true,
                    CommandRequired = false,
                    Help = "Path of assembly to check",
                    OnCommand = s => path = s
                };
            }
        }

        protected override int Run(string command) {
            if (String.IsNullOrEmpty(name) && String.IsNullOrEmpty(path)) {
                Out.WriteLine("you must define either the name or the path of the assembly to check");
                return -1;
            }

            Assembly assembly = null;

            if (!String.IsNullOrEmpty(name)) {
                if (name.Contains(',')) {
                    // assume that we have a strong name;
                    try {
                        assembly = Assembly.Load(name);
                    } catch (Exception e) {
                        Log.Error(e.Message, e);
                        Out.WriteLine("could not load {0}", name);
                        return -1;
                    }
                } else {
                    // assume that we have a partal name
                    try {
                        assembly = Assembly.LoadWithPartialName(name);
                    } catch (Exception e) {
                        Log.Error(e.Message, e);
                        Out.WriteLine("could not load {0}", name);
                        return -1;
                    }
                }
            }
            //get the settings object
            Type ApplicationSettingsBaseT = typeof(ApplicationSettingsBase);
            Type settingsT = assembly.GetTypes().Where(t => t.BaseType == ApplicationSettingsBaseT).FirstOrDefault();

            Log.InfoFormat("Found {0}", settingsT.FullName);

            if (settingsT == null) {
                Out.WriteLine("{0} has no settings object passes by default");
                return 0;
            }
            Type providerT = typeof(Provider);
            var attr = settingsT.GetCustomAttributes<SettingsProviderAttribute>(true).FirstOrDefault();

            if (attr == null || attr.ProviderTypeName != providerT.FullName) {
                Out.WriteLine("Error: {0} is not using the settings provider");
            }

            return 0;
        }
    }
}
