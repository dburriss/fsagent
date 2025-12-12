namespace FsAgent

type DataFormat =
    | Yaml
    | Json
    | Toon

type Node =
    | Text of string
    | Block of string
    | Section of name: string * content: Node list
    | List of Node list
    | Imported of sourcePath: string * format: DataFormat

type Agent = {
    Frontmatter: Map<string, obj>
    Sections: Node list
}

module AST =
    let role (text: string) : Node =
        Section("role", [Text text])

    let objective (text: string) : Node =
        Section("objective", [Text text])

    let instructions (text: string) : Node =
        Section("instructions", [Text text])

    let context (text: string) : Node =
        Section("context", [Text text])

    let output (text: string) : Node =
        Section("output", [Text text])

    let example (title: string) (content: string) : Node =
        Section("example", [Text title; Text content])

    let examples (examples: Node list) : Node =
        Section("examples", [List examples])

module Say =
    let hello name =
        printfn "Hello %s" name
