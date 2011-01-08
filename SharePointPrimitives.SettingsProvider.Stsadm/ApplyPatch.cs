using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharePointPrimitives.Stsadm;
using System.Xml.Linq;

namespace SharePointPrimitives.SettingsProvider.Stsadm {
    class ApplyPatch : BaseCommand {
        bool noPrompt;
        string filename;

        protected override IEnumerable<CommandArgument> CommandArguments {
            get { 
                yield return new CommandArgument() {
                   Name = "no-prompt",
                   ArgumentRequired = false,
                   CommandRequired = false,
                   Help = "command will run with out prompting",
                   OnCommand = _ => noPrompt = true
                };
                yield return new CommandArgument() {
                    Name = "filename",
                    ArgumentRequired = true,
                    CommandRequired = true,
                    Help = "path to the patch to apply",
                    OnCommand = s => filename = s
                };
            }
        }

        protected override int Run(string command) {
            var doc = XDocument.Load(filename);

            var patches = doc.Descendants("patch")
                .Select(elm => Patch.FromXml(elm))
                .ToList();

            if(!noPrompt){
                foreach(var patch in patches)
                    Console.WriteLine(patch);
                Console.Write("apply? (y)es >");
                if(!Console.ReadLine().ToLower().StartsWith("y"))
                    return 0;
            }

            foreach(var patch in patches)
                patch.Apply(new Patch.ApplyOptions());

            return 0;
        }
    }
}
