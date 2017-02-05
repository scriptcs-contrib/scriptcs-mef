[xml]$xml = Get-Content "coverage.xml" -Encoding UTF8
$xml.SelectNodes("//ModuleName[starts-with(., 'ℛ')]/..") | % { $_.ParentNode.RemoveChild($_) };

[System.IO.File]::WriteAllLines("R_BUILD_FOLDER\filtered-coverage.xml", $xml.OuterXml)