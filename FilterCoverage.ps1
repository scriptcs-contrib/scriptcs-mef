[xml]$xml = Get-Content "coverage.xml"
Write-Host ($xml | Format-Table | Out-String)

$xml.CoverageSession.Modules.Module | % { if ($_ -and $_.ModuleName -and $_.ModuleName.StartsWith('â„›*')) { $_.ParentNode.RemoveChild($_) } }

$fileName = "$env:APPVEYOR_BUILD_FOLDER\coverage.xml";
$xml.Save($fileName)
Write-Host "file saved at '$fileName'"