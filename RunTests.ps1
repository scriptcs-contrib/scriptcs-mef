if($env:APPVEYOR_PULL_REQUEST_NUMBER -eq $null) {
    OpenCover.Console.exe -register:user -filter:"+[ScriptCs.ComponentModel.Composition]*" -excludebyattribute:*.ExcludeFromCodeCoverage* -target:"$env:xunit20\xunit.console.x86.exe" -targetargs:"`"src\ScriptCs.ComponentModel.Composition.Test\bin\Release\ScriptCs.ComponentModel.Composition.Test.dll`" -noshadow -appveyor" -output:coverage.xml -returntargetcode
    if ($lastExitCode -ne 0)
    {
        Write-Error "Tests have failed"
    }
    Write-Information "Tests ok"
    coveralls.net --opencover coverage.xml
}
else
{
    & "$env:xunit20\xunit.console.x86" "src\ScriptCs.ComponentModel.Composition.Test\bin\Release\ScriptCs.ComponentModel.Composition.Test.dll" -noshadow -appveyor
    if ($lastExitCode -ne 0)
    {
        Write-Error "Tests have failed"
    }
    Write-Information "Tests ok"
}