OpenCover.Console.exe -register:user -filter:"+[ScriptCs.ComponentModel.Composition]*" -excludebyattribute:*.ExcludeFromCodeCoverage* -target:"$env:xunit20\xunit.console.x86.exe" -targetargs:"`"src\ScriptCs.ComponentModel.Composition.Test\bin\Release\ScriptCs.ComponentModel.Composition.Test.dll`" -noshadow -appveyor" -output:coverage.xml -returntargetcode
if ($lastExitCode -ne 0)
{
    Write-Error "Tests have failed"
    exit -1
}
Write-Information "Tests ok"
codecov -f coverage.xml