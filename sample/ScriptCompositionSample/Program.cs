using ScriptCs.ComponentModel.Composition;
using System;
using System.ComponentModel.Composition.Hosting;

namespace ScriptCompositionSample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Script args can be passed in so the script / script packs can access them.
            //For example the sample script, the logger script pack is used which depends on these args
            var scriptArgs = new string[] { "-loglevel", "INFO" };

            // You can add script by script
            //var catalog = new ScriptCsCatalog(new[] { "Test.csx" }, scriptArgs, typeof(IGreeter));

            // Or an entire folder

            var catalog = new ScriptCsCatalog("Scripts", scriptArgs, typeof(IGreeter));
            var container = new CompositionContainer(catalog);
            var greeter = container.GetExportedValue<IGreeter>();
            greeter.Greet("Hello MEF!");
            Console.ReadLine();
        }
    }
}