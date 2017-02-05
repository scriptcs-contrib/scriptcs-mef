let doc = new System.Xml.XmlDocument() in doc.LoadXml "coverage.xml"
doc.SelectNodes "//ModuleName[starts-with(., 'ℛ')]/.." |> Seq.iter (fun node -> node.ParentNode.RemoveChild(node))
System.IO.File.WriteAllLines("C:\Users\man-p\Downloads\filtered-coverage.xml", doc.OuterXml)