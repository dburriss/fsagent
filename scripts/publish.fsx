#!/usr/bin/env -S dotnet fsi

open System
open System.IO
open System.Text.RegularExpressions
open System.Diagnostics

// Helper to run git commands
let runGit args =
    let psi = ProcessStartInfo("git", args)
    psi.RedirectStandardOutput <- true
    psi.RedirectStandardError <- true
    psi.UseShellExecute <- false
    let p = Process.Start(psi)
    let output = p.StandardOutput.ReadToEnd()
    let error = p.StandardError.ReadToEnd()
    p.WaitForExit()
    if p.ExitCode <> 0 then
        failwithf "Git command failed: git %s\nError: %s" args error
    output.Trim()

// Paths
let rootDir = __SOURCE_DIRECTORY__ |> Directory.GetParent
let fsprojPath = Path.Combine(rootDir.FullName, "src/FsAgent/FsAgent.fsproj")
let changelogPath = Path.Combine(rootDir.FullName, "CHANGELOG.md")

printfn "Checking files..."
if not (File.Exists fsprojPath) then failwithf "Project file not found: %s" fsprojPath
if not (File.Exists changelogPath) then failwithf "Changelog not found: %s" changelogPath

// 1. Get current version from fsproj
let fsprojContent = File.ReadAllText(fsprojPath)
let versionRegex = Regex("<Version>(.*?)</Version>")
let versionMatch = versionRegex.Match(fsprojContent)
if not versionMatch.Success then failwith "Could not find <Version> in fsproj"

let currentVersionStr = versionMatch.Groups.[1].Value
let currentVersion = Version.Parse(currentVersionStr)

printfn "Current Version: %O" currentVersion

// 2. Parse CHANGELOG.md for Unreleased section
let changelogLines = File.ReadAllLines(changelogPath) |> Array.toList

let unreleasedHeaderIdx = 
    changelogLines 
    |> List.tryFindIndex (fun l -> l.StartsWith("## [Unreleased]"))

if unreleasedHeaderIdx.IsNone then failwith "Could not find '## [Unreleased]' in CHANGELOG.md"

let unreleasedIdx = unreleasedHeaderIdx.Value

// Find the next version header to determine the end of Unreleased section
let nextSectionIdx = 
    changelogLines 
    |> List.skip (unreleasedIdx + 1)
    |> List.tryFindIndex (fun l -> l.StartsWith("## [") && not (l.Contains("Unreleased")))
    |> Option.map (fun i -> i + unreleasedIdx + 1)
    |> Option.defaultValue changelogLines.Length

// Extract unreleased lines
let unreleasedContent = 
    changelogLines 
    |> List.skip (unreleasedIdx + 1)
    |> List.take (nextSectionIdx - unreleasedIdx - 1)
    |> List.filter (fun l -> not (String.IsNullOrWhiteSpace(l)))

printfn "\nUnreleased Changes:"
if unreleasedContent.IsEmpty then
    printfn "  (None)"
else
    unreleasedContent |> List.iter (fun l -> printfn "  %s" l)

printfn ""
if unreleasedContent.IsEmpty then
    printf "Warning: No unreleased changes found. Continue? (y/n): "
    if Console.ReadLine().ToLower() <> "y" then exit 0

// 3. Determine new version
printfn "Select increment type:"
let nextMajor = Version(currentVersion.Major + 1, 0, 0)
let nextMinor = Version(currentVersion.Major, currentVersion.Minor + 1, 0)
let nextPatch = Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build + 1)

printfn "1) Major (%O -> %O)" currentVersion nextMajor
printfn "2) Minor (%O -> %O)" currentVersion nextMinor
printfn "3) Patch (%O -> %O)" currentVersion nextPatch

printf "Choice (1-3): "
let choice = Console.ReadLine()
let newVersion = 
    match choice with
    | "1" -> nextMajor
    | "2" -> nextMinor
    | "3" -> nextPatch
    | _ -> failwith "Invalid selection"

printfn "New Version: %O" newVersion

// 4. Update fsproj
printfn "Updating fsproj..."

// Create release notes string (semicolon separated, stripped of markdown bullets)
let releaseNotes = 
    unreleasedContent
    |> List.filter (fun l -> l.Trim().StartsWith("-") || l.Trim().StartsWith("*"))
    |> List.map (fun l -> l.Trim().TrimStart('-', '*').Trim())
    |> String.concat "; "

let newFsprojContent = 
    fsprojContent
    // Update Version
    |> fun s -> versionRegex.Replace(s, sprintf "<Version>%O</Version>" newVersion)
    // Update PackageReleaseNotes
    |> fun s -> Regex.Replace(s, "<PackageReleaseNotes>.*?</PackageReleaseNotes>", sprintf "<PackageReleaseNotes>%s</PackageReleaseNotes>" releaseNotes, RegexOptions.Singleline)

File.WriteAllText(fsprojPath, newFsprojContent)

// 5. Update CHANGELOG.md
printfn "Updating CHANGELOG.md..."
let today = DateTime.Now.ToString("yyyy-MM-dd")

// Construct new lines:
// [Before Unreleased]
// ## [Unreleased]
//
// ## [NewVersion] - Date
// [Old Unreleased Content]
// [Rest of file]

let preUnreleasedLines = changelogLines |> List.take unreleasedIdx
let postUnreleasedLines = changelogLines |> List.skip (unreleasedIdx + 1)

let newChangelogSection = 
    [
        "## [Unreleased]";
        "";
        sprintf "## [%O] - %s" newVersion today
    ]

let newChangelogLines = 
    preUnreleasedLines @ 
    newChangelogSection @ 
    postUnreleasedLines

File.WriteAllLines(changelogPath, newChangelogLines)

// 6. Git Operations
printfn "\nFiles updated. Ready to commit."
printfn "Commands to be executed:"
printfn "1. git add \"%s\" \"%s\"" fsprojPath changelogPath
printfn "2. git commit -m \"release: prepare v%O\"" newVersion
printfn "3. git tag v%O" newVersion
printfn "4. git push origin main"
printfn "5. git push origin v%O" newVersion

printf "Proceed with git operations? (y/n): "
if Console.ReadLine().ToLower() = "y" then
    printfn "Executing git add..."
    runGit (sprintf "add \"%s\" \"%s\"" fsprojPath changelogPath) |> ignore
    
    printfn "Executing git commit..."
    runGit (sprintf "commit -m \"release: prepare v%O\"" newVersion) |> ignore
    
    printfn "Executing git tag..."
    runGit (sprintf "tag v%O" newVersion) |> ignore
    
    printf "Push to remote? (y/n): "
    if Console.ReadLine().ToLower() = "y" then
        printfn "Pushing main..."
        try 
            runGit "push origin main" |> printfn "%s"
            printfn "Pushing tag..."
            runGit (sprintf "push origin v%O" newVersion) |> printfn "%s"
            printfn "Done!"
        with ex ->
            printfn "Error pushing: %s" ex.Message
    else
        printfn "Push skipped. Don't forget to push manually!"
else
    printfn "Git operations skipped."
