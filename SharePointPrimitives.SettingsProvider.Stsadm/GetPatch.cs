using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharePointPrimitives.Stsadm;
using System.Reflection;
using System.Xml.Linq;
using SharePointPrimitives.SettingsProvider.Reflection;

namespace SharePointPrimitives.SettingsProvider.Stsadm {
    public class GetPatch : BaseCommand {

        string assemblyName;
        string assemblyPath;
        string sectionName;
        string output;

        protected override string HelpDescription {
            get {
                return
                    @"Validates that the settings for the assembly are in the database
you can use `stsadm -o settings-sync-with-assembly` to push defaults into the database";
            }
        }

        protected override IEnumerable<CommandArgument> CommandArguments {
            get {
                yield return new CommandArgument() {
                    Name = "assembly-name",
                    ArgumentRequired = true,
                    CommandRequired = false,
                    Help = "Full or Partal name of the assembly to load",
                    OnCommand = s => assemblyName = s
                };
                yield return new CommandArgument() {
                    Name = "assembly-path",
                    ArgumentRequired = true,
                    CommandRequired = false,
                    Help = "Path of assembly to check",
                    OnCommand = s => assemblyPath = s
                };
                yield return new CommandArgument() {
                    Name = "section-name",
                    ArgumentRequired = true,
                    CommandRequired = false,
                    Help = "name of the section to pull from the database, pulls the current state of the database to hand generate a patch",
                    OnCommand = s => sectionName = s
                };
                yield return new CommandArgument() {
                    Name = "output",
                    ArgumentRequired = true,
                    CommandRequired = true,
                    Help = "show the patch to bring the database back in sync if needed",
                    OnCommand = s => output = s
                };

            }
        }

        protected override int Run(string command) {
            if (String.IsNullOrEmpty(assemblyName) && String.IsNullOrEmpty(assemblyPath) && String.IsNullOrEmpty(sectionName)) {
                Out.WriteLine("you must define either the assembly name, the path of the assembly, xor the section name to pull form the database");
                return -1;
            }

            Patch patch = null;

            if (!String.IsNullOrEmpty(sectionName)) {
                SnapShot current = SnapShot.GetFor(sectionName);
                patch = new Patch(current, Patch.Action.ActionType.Update);
            } else {

                Assembly assembly = Tools.LoadAssembly(assemblyName, assemblyPath);

                if (assembly == null) {
                    Out.WriteLine("could not load {0}{1}", assemblyName, assemblyPath);
                    return -1;
                }

                if (!assembly.HasSettings()) {
                    Out.WriteLine("{0} has not settings no patch to generate", assembly.FullName);
                    return 0;
                }

                SnapShot assemblySettings = SnapShot.BuildFrom(assembly);
                SnapShot databaseSettings = SnapShot.GetFor(assembly);
                patch = new Patch(databaseSettings, assemblySettings);
            }
            patch.ToXml().Save(output, SaveOptions.None);

            return 0;
        }
    }
}
