using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharePointPrimitives.Stsadm;

namespace SharePointPrimitives.SettingsProvider.Stsadm {
    class Restore : BaseCommand {

        bool noPrompt;
        DateTime? timestamp;

        protected override IEnumerable<CommandArgument> CommandArguments {
            get {
                yield return new CommandArgument() {
                    Name = "rollback-to",
                    ArgumentRequired = true,
                    CommandRequired = true,
                    Help = "Time to roll back to",
                    OnCommand = s => {
                        try {
                            timestamp = DateTime.Parse(s);
                        } catch { } //errors in parsing are handeled in the run function
                    }
                };
                yield return new CommandArgument() {
                    Name = "no-prompt",
                    ArgumentRequired = false,
                    CommandRequired = false,
                    OnCommand = _ => noPrompt = true
                };
            }
        }

        protected override int Run(string command) {
            if(timestamp == null) {
                Out.WriteLine("could not parse the time to roll back to");
                return -1;
            }

            if (timestamp > DateTime.Now) {
                Out.WriteLine("this 'wayback' machine can only go back in time please choose a time in the past");
                return -1;
            }

            if (!noPrompt) {
                Console.Write("Rollback the settings back to what the were on {0}? (y)es > ", timestamp);
                if (!Console.ReadLine().ToLower().StartsWith("y"))
                    return 0;
            }
            using (var database = new SettingsProviderDatabase()) {
                database.ReloadFromAuditLog(timestamp.Value);
            }
            return 0;
        }
    }
}
