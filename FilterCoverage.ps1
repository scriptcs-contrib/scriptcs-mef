$MyFile = Get-Content "coverage.xml"
[System.IO.File]::WriteAllLines("$env:APPVEYOR_BUILD_FOLDER\coverage.xml", $MyFile)