[xml]$xml = Get-Content "coverage.xml" -Encoding UTF8
Write-Host $xml.OuterXml
$xml.CoverageSession.Modules.Module | % { Write-Host "$_ is examined"; if ($_ -and $_.ModuleName -and $_.ModuleName.StartsWith('ℛ*')) { Write-Host "$_.ModuleName removed"; $_.ParentNode.RemoveChild($_) } }

$fileName = "$env:APPVEYOR_BUILD_FOLDER\filtered-coverage.xml";
[System.IO.File]::WriteAllLines($fileName, $xml.OuterXml)
Write-Host "file saved at '$fileName'"
