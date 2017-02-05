[xml]$xml = Get-Content "coverage.xml" -Encoding UTF8

$xml.CoverageSession.Modules.Module | % { if ($_ -and $_.ModuleName -and $_.ModuleName.StartsWith('ℛ*')) { $_.ParentNode.RemoveChild($_) } }

$fileName = "$env:APPVEYOR_BUILD_FOLDER\filtered-coverage.xml";
[System.IO.File]::WriteAllLines($fileName, $xml.OuterXml)
Write-Host "file saved at '$fileName'"
