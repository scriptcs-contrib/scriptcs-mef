using ScriptCs.ComponentModel.Composition;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

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
            //var catalog = new ScriptCsCatalog(new[] { "Test.csx" }, typeof(IGreeter));
            var catalog = new ScriptCsCatalog("Scripts", typeof(IGreeter));
            var container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            batch.AddPart(this);
            container.Compose(batch);
        }
    }
}