using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace ScriptCs.ComponentModel.Composition.Test
{
    public class MEFHost
    {
        [ImportMany]
        public List<IDoSomething> Plugins { get; set; }
    }

    public interface IDoSomething
    {
        string DoSomething();
    }
}