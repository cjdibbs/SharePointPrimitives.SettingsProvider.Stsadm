using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;

namespace SharePointPrimitives.SettingsProvider.Stsadm.Test {
    [TestClass]
    public class ValidateAssemblyTests {
        [TestMethod]
        public void ValidateSimpleExamplea() {
            var command = new ValidateAssembly();
            string output;
            StringDictionary args = new StringDictionary();
            args["path"] = @"C:\Users\Chris\Desktop\SharePointPrimitives.SettingsProvider\SharePointPrimitives.SettingsProvider.SimpleExample\bin\Debug\SimpleExample.exe";
            command.Run(null, args, out output);
        }
    }
}
