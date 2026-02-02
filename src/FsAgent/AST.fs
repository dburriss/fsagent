namespace FsAgent.AST

type DataFormat =
    | Yaml
    | Json
    | Toon
    | Unknown

type Node =
    | Text of string
    | Section of name: string * content: Node list
    | List of Node list
    | Imported of sourcePath: string * format: DataFormat * wrapInCodeBlock: bool
    | Template of text: string
    | TemplateFile of path: string

module AST =
    let fmStr (value: string) : obj = value :> obj

    let fmNum (value: float) : obj = value :> obj

    let fmBool (value: bool) : obj = value :> obj

    let fmList (value: obj list) : obj = value :> obj

    let fmMap (value: Map<string, obj>) : obj = value :> obj

    let inferFormat (path: string) : DataFormat =
        match System.IO.Path.GetExtension(path).ToLower() with
        | ".yml" | ".yaml" -> Yaml
        | ".json" -> Json
        | ".toon" -> Toon
        | _ -> Unknown

    let importRef (path: string) : Node =
        Imported(path, inferFormat path, true)

    let importRawRef (path: string) : Node =
        Imported(path, inferFormat path, false)
