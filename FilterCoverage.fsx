let doc = new System.Xml.XmlDocument() in doc.Load "coverage.xml"
doc.SelectNodes "//ModuleName[starts-with(., 'ℛ')]/.."
    |> Seq.cast<System.Xml.XmlNode>
    |> Seq.iter (fun node -> node.ParentNode.RemoveChild(node) |> ignore)
    |> ignore
System.IO.File.WriteAllLines("$env:APPVEYOR_BUILD_FOLDER/filtered-coverage.xml", doc.OuterXml.Split(' '))