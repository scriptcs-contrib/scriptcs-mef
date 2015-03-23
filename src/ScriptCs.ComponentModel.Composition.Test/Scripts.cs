using System;

namespace ScriptCs.ComponentModel.Composition.Test
{
    internal static class Scripts
    {
        internal static string SimpleScript { get { return string.Format(_simpleScript, Environment.CurrentDirectory); } }

        private const string _simpleScript = @"#r ""{0}\ScriptCs.ComponentModel.Composition.Test.dll""
using ScriptCs.ComponentModel.Composition.Test;
[Export(typeof(IDoSomething))]
public class SimpleSomething : IDoSomething
{{
    public string DoSomething()
    {{
        return ""Simple"";
    }}
}}";

        internal static string DoubleScript { get { return string.Format(_doubleScript, Environment.CurrentDirectory); } }

        private const string _doubleScript = @"#r ""{0}\ScriptCs.ComponentModel.Composition.Test.dll""
using ScriptCs.ComponentModel.Composition.Test;
[Export(typeof(IDoSomething))]
public class DoubleSomething : IDoSomething
{{
    public string DoSomething()
    {{
        return ""Double"";
    }}
}}";

        internal const string SimpleScriptWithoutReference = @"[Export(typeof(IDoSomething))]
public class SimpleSomething : IDoSomething
{
    public string DoSomething()
    {
        return ""Simple"";
    }
}";

        internal const string CompileExceptionScript = @"
public class SimpleSomething
{
    public string DoSomething()
    {
        return ""Simple""
    }
}";

        internal const string ExecutionExceptionScript = @"
public class SimpleSomething
{
    public string DoSomething()
    {
        return ""Simple"";
    }
}
throw new Exception(""Exception from script execution"");";
    }
}