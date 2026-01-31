module AstTests

open Xunit
open FsAgent
open FsAgent.AST

[<Fact>]
let ``AST fmStr creates boxed string`` () =
    let obj = AST.fmStr "test"
    Assert.Equal("test", obj :?> string)

[<Fact>]
let ``AST fmNum creates boxed float`` () =
    let obj = AST.fmNum 42.0
    Assert.Equal(42.0, obj :?> float)

[<Fact>]
let ``AST fmBool creates boxed bool`` () =
    let obj = AST.fmBool true
    Assert.Equal(true, obj :?> bool)

[<Fact>]
let ``AST fmList creates boxed list`` () =
    let obj = AST.fmList ["a" :> obj; "b" :> obj]
    let list = obj :?> obj list
    Assert.Equal(2, list.Length)
    Assert.Equal("a", list[0] :?> string)

[<Fact>]
let ``AST fmMap creates boxed map`` () =
    let map = Map.ofList [("k", "v" :> obj)]
    let obj = AST.fmMap map
    let m = obj :?> Map<string, obj>
    Assert.Equal("v", m["k"] :?> string)

[<Fact>]
let ``AST inferFormat recognizes yaml`` () =
    Assert.Equal(Yaml, AST.inferFormat "file.yml")
    Assert.Equal(Yaml, AST.inferFormat "file.yaml")

[<Fact>]
let ``AST inferFormat recognizes json`` () =
    Assert.Equal(Json, AST.inferFormat "file.json")

[<Fact>]
let ``AST inferFormat recognizes toon`` () =
    Assert.Equal(Toon, AST.inferFormat "file.toon")

[<Fact>]
let ``AST inferFormat returns Unknown for unknown extension`` () =
    Assert.Equal(Unknown, AST.inferFormat "file.txt")

[<Fact>]
let ``AST importRef creates Imported node with inferred format and wrapInCodeBlock true`` () =
    let node = AST.importRef "data.yml"
    match node with
    | Imported(path, format, wrapInCodeBlock) ->
        Assert.Equal("data.yml", path)
        Assert.Equal(Yaml, format)
        Assert.True(wrapInCodeBlock)
    | _ -> Assert.Fail("Expected Imported node")

[<Fact>]
let ``AST importRawRef creates Imported node with wrapInCodeBlock false`` () =
    let node = AST.importRawRef "data.json"
    match node with
    | Imported(path, format, wrapInCodeBlock) ->
        Assert.Equal("data.json", path)
        Assert.Equal(Json, format)
        Assert.False(wrapInCodeBlock)
    | _ -> Assert.Fail("Expected Imported node")

[<Fact>]
let ``Template node contains text`` () =
    let node = Template "Hello {{{name}}}"
    match node with
    | Template text ->
        Assert.Equal("Hello {{{name}}}", text)
    | _ -> Assert.Fail("Expected Template node")

[<Fact>]
let ``TemplateFile node contains path`` () =
    let node = TemplateFile "templates/greeting.txt"
    match node with
    | TemplateFile path ->
        Assert.Equal("templates/greeting.txt", path)
    | _ -> Assert.Fail("Expected TemplateFile node")
