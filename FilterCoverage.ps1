[xml]$xml = Get-Content "coverage.xml" -Encoding UTF8
Write-Host ($xml | Format-Table | Out-String)

$xml.CoverageSession.Modules.Module | % { if ($_ -and $_.ModuleName -and $_.ModuleName.StartsWith('ℛ*')) { $_.ParentNode.RemoveChild($_) } }

$fileName = "$env:APPVEYOR_BUILD_FOLDER\coverage.xml";
[System.IO.File]::WriteAllLines("$env:APPVEYOR_BUILD_FOLDER\coverage.xml", $xml.OuterXml)
Write-Host "file saved at '$fileName'"
