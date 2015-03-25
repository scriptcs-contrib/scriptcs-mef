using ScriptCs.ComponentModel.Composition;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace ScriptCompositionSample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var program = new Program();
            program.Compose();

            program.Greeter.Greet("Hello MEF1!");
        }

        [Import]
        public IGreeter Greeter { get; set; }

        public void Compose()
        {
            // You can add script by script
            //var catalog = new ScriptCsCatalog(new[] { "Test.csx" }, typeof(IGreeter));

            // Or an entire folder
            InitializeScripts();
            var catalog = new ScriptCsCatalog("Scripts", typeof(IGreeter));
            var container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            batch.AddPart(this);
            container.Compose(batch);
        }

        private void InitializeScripts()
        {
            if (!Directory.Exists("Scripts"))
            {
                Directory.CreateDirectory("Scripts");
            }

            File.Copy("Test.csx", "Scripts/Test.csx");
        }
    }
}