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

namespace SharePointPrimitives.SettingsProvider.Stsadm {
    public class ValidateAssembly : BaseCommand {

        string name;
        string path;
        bool premoteWarnings = false;
        List<string> warnings = new List<string>();
        List<string> errors = new List<string>();

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
                yield return new CommandArgument() {
                    Name = "warnings-as-errors",
                    ArgumentRequired = false,
                    Help = "turn off of the warnings into errors",
                    OnCommand = _ => premoteWarnings = true
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
                try {
                    assembly = Assembly.LoadWithPartialName(name); // I am aware it is depercated but it is soo useful
                } catch (Exception e) {
                    Log.Error(e.Message, e);
                }
            } else {
                try {
                    assembly = Assembly.LoadFile(path);
                } catch (Exception e) {
                    Log.Error(e.Message, e);
                }
            }

            if (assembly == null) {
                Out.WriteLine("could not load {0}{1}", name,path);
                return -1;
            }

            //get the settings object
            Type ApplicationSettingsBaseT = typeof(ApplicationSettingsBase);
            Type settingsT = assembly.GetTypes().Where(t => t.BaseType == ApplicationSettingsBaseT).FirstOrDefault();



            if (settingsT == null) {
                Out.WriteLine("{0} has no settings object passes by default", assembly.FullName);
                return 0;
            }

            Log.InfoFormat("Found {0}", settingsT.FullName);

            Type providerT = typeof(Provider);
            var attr = settingsT.GetCustomAttribute<SettingsProviderAttribute>(true);

            if (attr == null || !attr.ProviderTypeName.StartsWith(providerT.FullName)) {
                errors.Add(String.Format("{0} is not using the settings provider", assembly.FullName));
            }

            //the name of the section is the full name of the class
            string section = settingsT.FullName;

            using (var database = new SettingsProviderDatabase()) {
                Dictionary<string,string> applicationCache = database.GetApplcationSettingsFor(section);
                Dictionary<string, string> connectionCache = database.GetNamedConnectionsFor(section);

                IEnumerable<PropertyInfo> settings = 
                    settingsT.GetProperties()
                    .Where(p => p.HasCustomAttribute<ApplicationScopedSettingAttribute>(true));

                foreach (PropertyInfo setting in settings) {
                    SpecialSettingAttribute special = setting.GetCustomAttribute<SpecialSettingAttribute>(true);
                    if (special == null) {
                        //normal settings 
                        if (!applicationCache.ContainsKey(setting.Name)) {
                            warnings.Add(String.Format("{0} will be loading the default value of '{1}'", setting.Name, GetDefaultValue(setting)));
                        }
                    } else if (special.SpecialSetting == SpecialSetting.ConnectionString) {
                        //connection string
                        string settingName = section + "." + setting.Name;

                        if (!connectionCache.ContainsKey(settingName))
                            errors.Add(String.Format("{0} will be loading the default value of '{1}'", settingName, GetDefaultValue(setting)));

                    }
                }
            }

            var report = new XElement("report");
            if (warnings.Any())
                report.Add(warnings.Select(w => new XElement(premoteWarnings ? "error" : "warning", w)).ToArray());
            if(errors.Any())
                report.Add(errors.Select(e => new XElement("error", e)).ToArray());

            report.WriteTo(new XmlTextWriter(Out));
            return 0;
        }

        private static string GetDefaultValue(PropertyInfo setting) {
            string value = null;

            DefaultSettingValueAttribute defaultValue = setting.GetCustomAttribute<DefaultSettingValueAttribute>(true);
            if (defaultValue != null)
                value = defaultValue.Value;
            return value;
        }
    }
}
