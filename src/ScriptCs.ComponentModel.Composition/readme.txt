Thanks to use ScriptCs.ComponentModel.Composition!

You can use it right now, you have a sample on GitHub: https://github.com/scriptcs-contrib/scriptcs-mef/blob/master/sample/ScriptCompositionSample/Program.cs

But sadly you one more thing to do before using it at runtime:
None of the ScriptCs engines needed to work have been included, because each one of them drag several dependencies which can be quite heavy.
So depending on the platform you are targeting, you need to install:
 - ScriptCs.Engine.Roslyn (Install-Package ScriptCs.Engine.Roslyn)
and / or 
 - ScriptCs.Engine.Mono  (Install-Package ScriptCs.Engine.Mono)

The ScriptCsCatalog will select automatically which one to use at runtime depending on the platform (Windows or Linux).

If you are experiencing some issues please first update the ScriptCs.Hosting package to the same version as ScriptCs.Engine.* package.
If you have still issues, please fill one on GitHub: https://github.com/scriptcs-contrib/scriptcs-mef/issues.