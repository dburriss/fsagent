module AstTests

open Xunit
open FsAgent
open FsAgent.DSL

[<Fact>]
let ``AST role constructor creates correct Section`` () =
    let node = AST.role "You are a .NET Version Upgrade Assistant, an expert in migrating .NET projects to newer versions while maintaining compatibility and resolving breaking changes."
    match node with
    | Section(name, content) ->
        Assert.Equal("role", name)
        match content with
        | [Text text] -> Assert.Equal("You are a .NET Version Upgrade Assistant, an expert in migrating .NET projects to newer versions while maintaining compatibility and resolving breaking changes.", text)
        | _ -> Assert.Fail("Expected single Text node")
    | _ -> Assert.Fail("Expected Section node")

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
let ``AST importRef creates Imported node with inferred format`` () =
    let node = AST.importRef "data.yml"
    match node with
    | Imported(path, format) ->
        Assert.Equal("data.yml", path)
        Assert.Equal(Yaml, format)
    | _ -> Assert.Fail("Expected Imported node")


[<Fact>]
let ``AST instructions constructor creates correct Section`` () =
    let instructionsText = "1. Analyze the current .NET version in .csproj/.fsproj files and global.json.\n2. Check for deprecated APIs and breaking changes between versions.\n3. Update TargetFramework in project files.\n4. Update NuGet packages to compatible versions.\n5. Run dotnet restore and build to identify compilation errors.\n6. Fix any code incompatibilities (e.g., API changes in ASP.NET Core).\n7. Update Docker files if applicable.\n8. Run tests and validate functionality.\n9. Document changes and potential migration notes."
    let node = AST.instructions instructionsText
    match node with
    | Section(name, content) ->
        Assert.Equal("instructions", name)
        match content with
        | [Text text] -> Assert.Equal(instructionsText, text)
        | _ -> Assert.Fail("Expected single Text node")
    | _ -> Assert.Fail("Expected Section node")

[<Fact>]
let ``AST context constructor creates correct Section`` () =
    let node = AST.context "The codebase is a typical .NET application with multiple projects, using packages like Entity Framework, ASP.NET Core, and various NuGet dependencies. The upgrade must consider LTS versions, security patches, and performance improvements."
    match node with
    | Section(name, content) ->
        Assert.Equal("context", name)
        match content with
        | [Text text] -> Assert.Equal("The codebase is a typical .NET application with multiple projects, using packages like Entity Framework, ASP.NET Core, and various NuGet dependencies. The upgrade must consider LTS versions, security patches, and performance improvements.", text)
        | _ -> Assert.Fail("Expected single Text node")
    | _ -> Assert.Fail("Expected Section node")

[<Fact>]
let ``AST output constructor creates correct Section`` () =
    let outputText = "Provide a summary of changes made, including:\n- Updated project files\n- Modified dependencies\n- Code changes required\n- Test results\n- Any remaining manual steps or warnings\n\nFormat the output as a markdown report with sections for each project updated."
    let node = AST.output outputText
    match node with
    | Section(name, content) ->
        Assert.Equal("output", name)
        match content with
        | [Text text] -> Assert.Equal(outputText, text)
        | _ -> Assert.Fail("Expected single Text node")
    | _ -> Assert.Fail("Expected Section node")

[<Fact>]
let ``AST example constructor creates correct Section`` () =
    let ex = AST.example "Example Title" "Example content"
    match ex with
    | Section(name, content) ->
        Assert.Equal("example", name)
        match content with
        | [Text "Example Title"; Text "Example content"] -> ()
        | _ -> Assert.Fail("Expected title and content texts")
    | _ -> Assert.Fail("Expected Section node")

[<Fact>]
let ``AST examples constructor creates correct Section with List`` () =
    let ex1 = AST.example "Upgrading from .NET 6 to .NET 8" "Successfully upgraded project with updated dependencies"
    let ex2 = AST.example "Handling breaking changes in ASP.NET Core" "Modern ASP.NET Core 8 application structure"
    let examplesNode = AST.examples [ex1; ex2]
    match examplesNode with
    | Section(name, content) ->
        Assert.Equal("examples", name)
        match content with
        | [List examples] ->
            match examples with
            | [Section("example", _); Section("example", _)] -> ()
            | _ -> Assert.Fail("Expected two example sections")
        | _ -> Assert.Fail("Expected single List node")
    | _ -> Assert.Fail("Expected Section node")

