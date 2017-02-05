[xml]$xml = Get-Content "coverage.xml" -Encoding UTF8

$xml.CoverageSession.Modules.Module | % { if ($_ -and $_.ModuleName -and $_.ModuleName.StartsWith('ℛ*')) { $_.ParentNode.RemoveChild($_) } }

$xml.Save("$env:APPVEYOR_BUILD_FOLDER\coverage.xml")