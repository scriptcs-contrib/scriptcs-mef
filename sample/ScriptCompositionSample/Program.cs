using ScriptCs.ComponentModel.Composition;
using System;
using System.ComponentModel.Composition.Hosting;

namespace ScriptCompositionSample
{
    internal static class Program
    {
        private static void Main()
        {
            // Script args can be passed in so the script / script packs can access them.
            // For example the sample script, the logger script pack is used which depends on these args
            var scriptArgs = new string[] { "-loglevel", "INFO" };
            var options = new ScriptCsCatalogOptions { ScriptArgs = scriptArgs, References = new[] { typeof(IGreeter) } };

            // You can add script by script
            //var catalog = new ScriptCsCatalog(new[] { "Scripts/Test.csx" }, options);

            // Or an entire folder
            var catalog = new ScriptCsCatalog("Scripts", options);

            // Script Packs can be used, for a sample you just have to:
            // - run command "scriptcs -install" in the Scripts folder of the folder, not the one in bin/Debug
            // - uncomment the commented code in Test.csx
            // If you want to test it with the "script by script" constructor, you need to copy the scriptcs_packages.config file
            // in the bin/Debug folder and run the "scriptcs -install" command

            var container = new CompositionContainer(catalog);
            var greeter = container.GetExportedValue<IGreeter>();
            greeter.Greet("Hello MEF!");
            Console.ReadLine();
        }
    }
}