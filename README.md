[![Licence Apache 2](https://img.shields.io/badge/licence-Apache%202-blue.svg)](https://github.com/scriptcs-contrib/scriptcs-mef/blob/master/LICENSE) [![Build status](https://ci.appveyor.com/api/projects/status/43y2p8xpsryqf40p?svg=true)](https://ci.appveyor.com/project/laedit/scriptcs-mef) [![Coverage Status](https://coveralls.io/repos/scriptcs-contrib/scriptcs-mef/badge.svg)](https://coveralls.io/r/scriptcs-contrib/scriptcs-mef) [![NuGet](https://img.shields.io/nuget/v/ScriptCs.ComponentModel.Composition.svg)](https://www.nuget.org/packages/ScriptCs.ComponentModel.Composition/)

![Project icon](icon.png)

# ScriptCs.ComponentModel.Composition

Add a ScriptCsCatalog which can be used to add some [ScriptCs](http://scriptcs.net/) scripts to a MEF container.

## Usage
Simply create a new `ScriptCsCatalog` and use it in your MEF composition:
```cs
// You can add script by script
//var catalog = new ScriptCsCatalog(new[] { "ScriptA.csx", "ScriptB.csx" });

// Or an entire folder
var catalog = new ScriptCsCatalog("Scripts");

var container = new CompositionContainer(catalog);
var batch = new CompositionBatch();
batch.AddPart(this);
container.Compose(batch);
```

**Add References**

It can be useful to add a references to all scripts loaded by the `ScriptCsCatalog`.
For example, you provide the interface `IGreeter` for imports in MEF.
So a basic script should be:
```cs
#r "Greeter.Contracts.dll"

using Greeter.Contracts;

public class MyGreeter : IGreeter
{
...
}
```

But you can pass the `IGreeter` type as reference to the `ScriptCsCatalog`:
```cs
// You can add script by script
var catalog = new ScriptCsCatalog(new[] { "ScriptA.csx", "ScriptB.csx" }, new ScriptCsCatalogOptions { References = new[] { typeof(IGreeter) } });

// Or an entire folder
var catalog = new ScriptCsCatalog("Scripts", new ScriptCsCatalogOptions { References = new[] typeof(IGreeter) } });
```
And the script will become:
```cs
public class MyGreeter : IGreeter
{
...
}
```

**Script args**

It is also possible to passes arguments to the script with the ScriptArgs property of the options:
```cs
var catalog = new ScriptCsCatalog("Scripts", new ScriptCsCatalogOptions { ScriptArgs = new[] { "-loglevel", "INFO" } });
```
It can be helpful with the use of Script Packs.

**Script Packs**

Script Packs can be used, the location varies depending on the approach used:
 - script by script: the packages location is the current directory of your application
 - folder: the packages location is the folder specified

**Load script one by one**

By default all scripts are loaded in one file to be interpreted by ScriptCS, but it can be source of issues like when [alias are used](https://github.com/scriptcs-contrib/scriptcs-mef/issues/10).

The `KeepScriptsSeparated` option allow to load script one by one and remove these issues:

```cs
var catalog = new ScriptCsCatalog("Scripts", new ScriptCsCatalogOptions { KeepScriptsSeparated = true });
```

## Contributors

Thanks to [Glenn Block](https://github.com/glennblock) Script Packs can be used!  
Thanks to [beolutz](https://github.com/beolutz) for the custom file system support and KeepScriptsSeparated option.
