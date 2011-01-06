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
using System.Xml.Linq;
using System.Xml;
using SharePointPrimitives.SettingsProvider.Reflection;

namespace SharePointPrimitives.SettingsProvider.Stsadm {
    public static class ListExtensions {
        public static void Add(this List<string> list, string format, params object[] items) {
            list.Add(string.Format(format, items));
        }
    }

    public class ValidateAssembly : BaseCommand {

        string name;
        string path;
        bool showPatch = false;

        protected override string HelpDescription {
            get { return 
@"Validates that the settings for the assembly are in the database
you can use `stsadm -o settings-sync-with-assembly` to push defaults into the database";
            }
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
                yield return new CommandArgument() {
                    Name = "show-patch",
                    ArgumentRequired = false,
                    Help = "show the patch to bring the database back in sync if needed",
                    OnCommand = _ => showPatch = true
                };

            }
        }

        protected override int Run(string command) {
            if (String.IsNullOrEmpty(name) && String.IsNullOrEmpty(path)) {
                Out.WriteLine("you must define either the name or the path of the assembly to check");
                return -1;
            }

            Assembly assembly = Tools.LoadAssembly(name, path);

            if (assembly == null) {
                Out.WriteLine("could not load {0}{1}", name,path);
                return -1;
            }

            if (!assembly.HasSettings()) {
                Out.WriteLine("{0} has no settings object passes by default", assembly.FullName);
                return 0;
            }

            SnapShot assemblySettings = SnapShot.BuildFrom(assembly);
            SnapShot databaseSettings = SnapShot.GetFor(assembly);

            Patch patch = new Patch(databaseSettings, assemblySettings);

            if (patch.IsEmpty) {
                Out.WriteLine("All of the settings in this assembly are in the database");
                return 0;
            }

            Out.WriteLine("The assembly and the database are out of sync run 'stsadm -o settings-sync-with-assembly' to sync");
            if(showPatch)
                Out.WriteLine(patch.ToXml().ToString(SaveOptions.None));
            return 1;
        }
    }
}
