using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharePointPrimitives.Stsadm;
using System.Reflection;

namespace SharePointPrimitives.SettingsProvider.Stsadm {
    class SyncSettings : BaseCommand {

        string name;
        string path;

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

            Assembly assembly = Tools.LoadAssembly(name, path);
            Type settingsT = Tools.GetSettings(assembly);

            if (settingsT == null) {
                Out.WriteLine("{0} does not use the custom settings provider nothing to sync", assembly.FullName);
                return 0;
            }



            return 0;
        }
    }
}
