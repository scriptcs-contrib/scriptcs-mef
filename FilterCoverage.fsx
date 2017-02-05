let doc = new System.Xml.XmlDocument() in doc.Load "coverage.xml"
doc.SelectNodes "//ModuleName[starts-with(., 'ℛ')]/.."
    |> Seq.cast<System.Xml.XmlNode>
    |> Seq.iter (fun node -> node.ParentNode.RemoveChild(node) |> ignore)
    |> ignore
let filteredFileName = System.String.Concat(System.Environment.GetEnvironmentVariable("APPVEYOR_BUILD_FOLDER"), "filtered-coverage.xml")
System.Console.WriteLine(filteredFileName)
System.IO.File.WriteAllLines(filteredFileName, doc.OuterXml.Split(' '))