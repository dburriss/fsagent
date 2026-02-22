module FsAgent.Toon.ToonParser

open System
open System.Text

// ─── Data model ──────────────────────────────────────────────────────────────

type ToonValue =
    | TString  of string
    | TNumber  of string      // keep as string to preserve original notation
    | TBool    of bool
    | TNull
    | TObject  of (string * ToonValue) list   // ordered key-value pairs
    | TArray   of ToonValue list

// ─── Parse error ─────────────────────────────────────────────────────────────

exception ToonParseException of line: int * message: string

// ─── Helpers ─────────────────────────────────────────────────────────────────

let private indentOf (s: string) =
    let mutable i = 0
    while i < s.Length && s.[i] = ' ' do i <- i + 1
    i

let private depthOf (s: string) (indentSize: int) =
    indentOf s / indentSize

// Split a string on the active delimiter, respecting double-quoted regions.
let private splitDelimited (delim: char) (s: string) : string list =
    let tokens = System.Collections.Generic.List<string>()
    let current = StringBuilder()
    let mutable inQuotes = false
    let mutable i = 0
    while i < s.Length do
        let c = s.[i]
        if c = '"' then
            inQuotes <- not inQuotes
            current.Append(c) |> ignore
        elif c = '\\' && inQuotes && i + 1 < s.Length then
            current.Append(c) |> ignore
            current.Append(s.[i+1]) |> ignore
            i <- i + 1
        elif c = delim && not inQuotes then
            tokens.Add(current.ToString().Trim())
            current.Clear() |> ignore
        else
            current.Append(c) |> ignore
        i <- i + 1
    tokens.Add(current.ToString().Trim())
    tokens |> Seq.toList

// Unescape a quoted string (only the five spec escapes).
let private unescapeString (lineNo: int) (raw: string) : string =
    // raw includes surrounding quotes
    if raw.Length < 2 || raw.[0] <> '"' || raw.[raw.Length-1] <> '"' then
        raise (ToonParseException(lineNo, $"Unterminated string: {raw}"))
    let inner = raw.[1..raw.Length-2]
    let sb = StringBuilder()
    let mutable i = 0
    while i < inner.Length do
        if inner.[i] = '\\' then
            if i + 1 >= inner.Length then
                raise (ToonParseException(lineNo, "Unterminated escape sequence"))
            match inner.[i+1] with
            | '\\' -> sb.Append('\\') |> ignore; i <- i + 2
            | '"'  -> sb.Append('"')  |> ignore; i <- i + 2
            | 'n'  -> sb.Append('\n') |> ignore; i <- i + 2
            | 'r'  -> sb.Append('\r') |> ignore; i <- i + 2
            | 't'  -> sb.Append('\t') |> ignore; i <- i + 2
            | c    -> raise (ToonParseException(lineNo, $"Invalid escape sequence: \\{c}"))
        else
            sb.Append(inner.[i]) |> ignore
            i <- i + 1
    sb.ToString()

// Parse a single primitive token.
let private parsePrimitive (lineNo: int) (token: string) : ToonValue =
    let t = token.Trim()
    if t.Length > 0 && t.[0] = '"' then
        TString (unescapeString lineNo t)
    elif t = "true" then TBool true
    elif t = "false" then TBool false
    elif t = "null" then TNull
    else
        // Try number: standard decimal, no forbidden leading zeros
        let mutable d = 0.0
        let isLeadingZero =
            t.Length > 1 && t.[0] = '0' && Char.IsDigit(t.[1])
        if not isLeadingZero && Double.TryParse(t, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, &d) then
            TNumber t
        else
            TString t

// ─── Array header parsing ────────────────────────────────────────────────────

type private ArrayHeader = {
    Key      : string option
    Length   : int
    Delim    : char
    Fields   : string list  // empty = not tabular
    Inline   : string       // text after the colon (trimmed)
}

// Attempt to parse an array header from a trimmed line.
// Returns Some header if valid, None otherwise.
let private tryParseArrayHeader (lineNo: int) (trimmed: string) : ArrayHeader option =
    // Pattern: [key]?[#?N[delim?]][{fields}]?:[ inline]?
    let mutable pos = 0
    let len = trimmed.Length

    // Optional key before '['
    let key =
        if len > 0 && trimmed.[0] <> '[' then
            let bracketIdx = trimmed.IndexOf('[')
            if bracketIdx <= 0 then None
            else
                let k = trimmed.[0..bracketIdx-1].TrimEnd()
                if k.Length = 0 then None else Some k
        else None

    pos <- match key with
           | Some k -> k.Length + (if key.IsSome && pos < len && trimmed.[pos+k.Length] = ' ' then 1 else 0)
           | None -> 0
    // Advance to '['
    let bracketStart = trimmed.IndexOf('[', pos)
    if bracketStart < 0 then None
    else

    let bracketEnd = trimmed.IndexOf(']', bracketStart)
    if bracketEnd < 0 then None
    else

    // Parse inside brackets: optional '#', digits, optional delim
    let bracketContent = trimmed.[bracketStart+1..bracketEnd-1]
    let mutable bc = bracketContent
    if bc.StartsWith("#") then bc <- bc.[1..]
    let delimChar, bcDigits =
        if bc.Length > 0 && (bc.[bc.Length-1] = '|' || bc.[bc.Length-1] = '\t') then
            bc.[bc.Length-1], bc.[0..bc.Length-2]
        else ',', bc

    let mutable arrLen = 0
    if not (Int32.TryParse(bcDigits, &arrLen)) then None
    else

    pos <- bracketEnd + 1

    // Optional fields segment {…}
    let fields, posAfterFields =
        if pos < len && trimmed.[pos] = '{' then
            let closeBrace = trimmed.IndexOf('}', pos)
            if closeBrace < 0 then raise (ToonParseException(lineNo, "Unclosed '{' in array header"))
            let fieldStr = trimmed.[pos+1..closeBrace-1]
            let fs = splitDelimited delimChar fieldStr |> List.map (fun f -> f.Trim())
            fs, closeBrace + 1
        else [], pos

    pos <- posAfterFields

    // Must have ':' next
    if pos >= len || trimmed.[pos] <> ':' then None
    else

    pos <- pos + 1
    let inline_ = if pos < len then trimmed.[pos..].TrimStart() else ""

    Some {
        Key    = key
        Length = arrLen
        Delim  = delimChar
        Fields = fields
        Inline = inline_
    }

// ─── Indexed lines ───────────────────────────────────────────────────────────

type private Line = { No: int; Raw: string; Depth: int; Trimmed: string }

let private toIndexedLines (input: string) (indentSize: int) : Line array =
    input.Split('\n')
    |> Array.mapi (fun i raw ->
        let r = raw.TrimEnd('\r')
        let d = if r.Trim() = "" then -1 else depthOf r indentSize
        { No = i + 1; Raw = r; Depth = d; Trimmed = r.TrimStart() })

// ─── Parser state ────────────────────────────────────────────────────────────

// Recursive descent parser over the indexed line array.
// pos is the current position (ref).

let rec private parseObject (lines: Line array) (pos: byref<int>) (depth: int) (lineNo: int) : (string * ToonValue) list =
    let pairs = System.Collections.Generic.List<string * ToonValue>()
    while pos < lines.Length && (lines.[pos].Depth = -1 || lines.[pos].Depth >= depth) do
        let line = lines.[pos]
        if line.Depth = -1 then
            pos <- pos + 1  // skip blank
        elif line.Depth > depth then
            raise (ToonParseException(line.No, $"Unexpected indentation (depth {line.Depth}, expected {depth})"))
        elif line.Depth = depth then
            if line.Trimmed.StartsWith("- ") || line.Trimmed = "-" then
                // List item at this depth — caller should handle
                ()
                pos <- lines.Length  // signal caller to stop
            else
                let key, value = parseField lines &pos depth line
                pairs.Add((key, value))
        else
            // depth < expected — done
            ()
            pos <- lines.Length  // force exit

    pairs |> Seq.toList

and private parseField (lines: Line array) (pos: byref<int>) (depth: int) (line: Line) : string * ToonValue =
    let trimmed = line.Trimmed

    // Try array header first
    match tryParseArrayHeader line.No trimmed with
    | Some hdr ->
        let key = hdr.Key |> Option.defaultWith (fun () -> raise (ToonParseException(line.No, "Array header at object depth must have a key")))
        pos <- pos + 1
        let arr = parseArrayBody lines &pos depth hdr line.No
        key, arr
    | None ->
        // Must be key: value
        let colonIdx = findFirstUnquotedColon trimmed line.No
        if colonIdx < 0 then raise (ToonParseException(line.No, $"Missing colon in line: {trimmed}"))
        let key = trimmed.[0..colonIdx-1].Trim()
        let rest = trimmed.[colonIdx+1..].TrimStart()
        pos <- pos + 1
        if rest = "" then
            // Nested object
            let nested = parseObjectAtDepth lines &pos (depth + 1) line.No
            key, TObject nested
        else
            key, parsePrimitive line.No rest

and private findFirstUnquotedColon (s: string) (lineNo: int) : int =
    let mutable inQ = false
    let mutable i = 0
    let mutable found = -1
    while i < s.Length && found = -1 do
        match s.[i] with
        | '"' -> inQ <- not inQ
        | '\\' when inQ -> i <- i + 1  // skip escape
        | ':' when not inQ -> found <- i
        | _ -> ()
        i <- i + 1
    found

and private parseObjectAtDepth (lines: Line array) (pos: byref<int>) (depth: int) (lineNo: int) : (string * ToonValue) list =
    let pairs = System.Collections.Generic.List<string * ToonValue>()
    let mutable stop = false
    while not stop && pos < lines.Length do
        let line = lines.[pos]
        if line.Depth = -1 then
            pos <- pos + 1
        elif line.Depth < depth then
            stop <- true
        elif line.Depth > depth then
            raise (ToonParseException(line.No, $"Unexpected indentation (depth {line.Depth}, expected {depth})"))
        else
            let key, value = parseField lines &pos depth line
            pairs.Add((key, value))
    pairs |> Seq.toList

and private parseArrayBody (lines: Line array) (pos: byref<int>) (parentDepth: int) (hdr: ArrayHeader) (lineNo: int) : ToonValue =
    let itemDepth = parentDepth + 1

    if hdr.Fields.Length > 0 then
        // Tabular array
        let rows = System.Collections.Generic.List<ToonValue>()
        let mutable stop = false
        while not stop && pos < lines.Length do
            let line = lines.[pos]
            if line.Depth = -1 then pos <- pos + 1
            elif line.Depth < itemDepth then stop <- true
            elif line.Depth > itemDepth then raise (ToonParseException(line.No, $"Unexpected indentation in tabular row"))
            else
                let values = splitDelimited hdr.Delim line.Trimmed |> List.map (parsePrimitive line.No)
                if values.Length <> hdr.Fields.Length then
                    raise (ToonParseException(line.No, $"Tabular row has {values.Length} values but {hdr.Fields.Length} fields declared"))
                let obj = List.zip hdr.Fields values
                rows.Add(TObject obj)
                pos <- pos + 1
        TArray (rows |> Seq.toList)

    elif hdr.Inline <> "" then
        // Primitive inline array
        let tokens = splitDelimited hdr.Delim hdr.Inline
        TArray (tokens |> List.map (parsePrimitive lineNo))

    else
        // Expanded list items
        let items = System.Collections.Generic.List<ToonValue>()
        let mutable stop = false
        while not stop && pos < lines.Length do
            let line = lines.[pos]
            if line.Depth = -1 then pos <- pos + 1
            elif line.Depth < itemDepth then stop <- true
            elif line.Depth > itemDepth then raise (ToonParseException(line.No, $"Unexpected indentation in list item"))
            else
                let item = parseListItem lines &pos itemDepth line
                items.Add(item)
        TArray (items |> Seq.toList)

and private parseListItem (lines: Line array) (pos: byref<int>) (depth: int) (line: Line) : ToonValue =
    // line.Trimmed starts with "- " or is "-"
    let content =
        if line.Trimmed = "-" then ""
        else line.Trimmed.[2..]  // strip "- "
    pos <- pos + 1

    if content = "" then
        TObject []  // empty object list item

    // Check for inner array header "- [M]: …"
    elif content.[0] = '[' then
        match tryParseArrayHeader line.No content with
        | Some innerHdr ->
            parseArrayBody lines &pos depth innerHdr line.No
        | None ->
            parsePrimitive line.No content

    else
        // Try to parse as object (key: value on hyphen line, more fields below)
        let colonIdx = findFirstUnquotedColon content line.No
        if colonIdx < 0 then
            // Primitive
            parsePrimitive line.No content
        else
            let firstKey = content.[0..colonIdx-1].Trim()
            let firstRest = content.[colonIdx+1..].TrimStart()

            // Collect sibling fields at depth+1 (below hyphen line)
            let siblings = System.Collections.Generic.List<string * ToonValue>()

            // Parse first field value
            let firstValue =
                if firstRest = "" then
                    // Nested object: fields at depth+2
                    let nested = parseObjectAtDepth lines &pos (depth + 2) line.No
                    TObject nested
                else
                    parsePrimitive line.No firstRest

            // Collect remaining sibling fields at depth+1
            let mutable stop = false
            while not stop && pos < lines.Length do
                let next = lines.[pos]
                if next.Depth = -1 then pos <- pos + 1
                elif next.Depth < depth + 1 then stop <- true
                elif next.Depth > depth + 1 then raise (ToonParseException(next.No, $"Unexpected indentation in list object"))
                elif next.Trimmed.StartsWith("- ") || next.Trimmed = "-" then
                    stop <- true  // next list item
                else
                    let k, v = parseField lines &pos (depth + 1) next
                    siblings.Add((k, v))

            let allFields = (firstKey, firstValue) :: (siblings |> Seq.toList)
            TObject allFields

// ─── Top-level parse ─────────────────────────────────────────────────────────

let parse (input: string) : ToonValue =
    let indentSize = 2
    let lines = toIndexedLines input indentSize

    // Skip leading blanks
    let firstNonBlank =
        lines |> Array.tryFind (fun l -> l.Depth <> -1)

    match firstNonBlank with
    | None -> TObject []  // empty document

    | Some first ->
        // Root array?
        match tryParseArrayHeader first.No first.Trimmed with
        | Some hdr when hdr.Key = None ->
            let mutable pos = 1
            parseArrayBody lines &pos 0 hdr first.No
        | _ ->
            // Object or primitive
            let mutable pos = 0
            let pairs = parseObjectAtDepth lines &pos 0 0
            match pairs with
            | [(_, v)] when (match v with TObject _ | TArray _ -> false | _ -> true) ->
                v  // single primitive root
            | _ ->
                TObject pairs

// ─── Encoder ─────────────────────────────────────────────────────────────────

// Quoting rules per spec §7.2
let private mustQuoteValue (delim: char) (s: string) : bool =
    s = "" ||
    s <> s.TrimStart() || s <> s.TrimEnd() ||
    s = "true" || s = "false" || s = "null" ||
    s = "-" || s.StartsWith("-") ||
    s.Contains(":") || s.Contains("\"") || s.Contains("\\") ||
    s.Contains("[") || s.Contains("]") || s.Contains("{") || s.Contains("}") ||
    s.Contains("\n") || s.Contains("\r") || s.Contains("\t") ||
    (delim <> ',' && s.Contains(string delim)) ||
    (delim = ',' && s.Contains(",")) ||
    // numeric-like
    (let mutable d = 0.0 in Double.TryParse(s, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, &d))

let private escapeString (s: string) : string =
    let sb = StringBuilder()
    sb.Append('"') |> ignore
    for c in s do
        match c with
        | '\\' -> sb.Append("\\\\") |> ignore
        | '"'  -> sb.Append("\\\"") |> ignore
        | '\n' -> sb.Append("\\n")  |> ignore
        | '\r' -> sb.Append("\\r")  |> ignore
        | '\t' -> sb.Append("\\t")  |> ignore
        | _    -> sb.Append(c) |> ignore
    sb.Append('"') |> ignore
    sb.ToString()

let private encodeValue (delim: char) (v: ToonValue) : string =
    match v with
    | TString s  -> if mustQuoteValue delim s then escapeString s else s
    | TNumber n  -> n
    | TBool true  -> "true"
    | TBool false -> "false"
    | TNull       -> "null"
    | TObject _   -> failwith "encodeValue: cannot inline an object"
    | TArray _    -> failwith "encodeValue: cannot inline an array"

// Check whether a list of (key,value) pairs is tabular-eligible:
// all values are primitives (no nested obj/arr) and all have the same keys.
let private isTabular (rows: (string * ToonValue) list list) : string list option =
    match rows with
    | [] -> None
    | first :: rest ->
        let fields = first |> List.map fst
        let allPrimitive =
            rows |> List.forall (fun row ->
                row |> List.forall (fun (_, v) ->
                    match v with
                    | TObject _ | TArray _ -> false
                    | _ -> true))
        let sameKeys =
            rest |> List.forall (fun row ->
                let ks = row |> List.map fst
                ks = fields)
        if allPrimitive && sameKeys then Some fields else None

let rec private encode (sb: StringBuilder) (indent: int) (delim: char) (v: ToonValue) : unit =
    let pad = String.replicate (indent * 2) " "
    match v with
    | TObject pairs ->
        for (k, vv) in pairs do
            let key = if Text.RegularExpressions.Regex.IsMatch(k, @"^[A-Za-z_][\w.]*$") then k else escapeString k
            match vv with
            | TObject _ ->
                sb.AppendLine($"{pad}{key}:") |> ignore
                encode sb (indent + 1) delim vv
            | TArray items ->
                encodeArray sb indent delim key items
            | _ ->
                sb.AppendLine($"{pad}{key}: {encodeValue delim vv}") |> ignore

    | TArray items ->
        encodeArray sb indent delim "" items

    | _ ->
        sb.AppendLine($"{pad}{encodeValue delim v}") |> ignore

and private encodeArray (sb: StringBuilder) (indent: int) (delim: char) (key: string) (items: ToonValue list) : unit =
    let pad = String.replicate (indent * 2) " "
    let n = items.Length
    let keyPrefix = if key = "" then "" else $"{key}"
    let delimSuffix = if delim = '\t' then "\t" elif delim = '|' then "|" else ""
    let lenMarker = $"[{n}{delimSuffix}]"

    match items with
    | [] ->
        sb.AppendLine($"{pad}{keyPrefix}{lenMarker}:") |> ignore

    | _ when items |> List.forall (fun i -> match i with TObject _ -> false | TArray _ -> false | _ -> true) ->
        // Primitive inline array
        let encoded = items |> List.map (encodeValue delim) |> String.concat (string delim)
        sb.AppendLine($"{pad}{keyPrefix}{lenMarker}: {encoded}") |> ignore

    | _ ->
        // Check tabular
        let objRows = items |> List.choose (function TObject pairs -> Some pairs | _ -> None)
        if objRows.Length = items.Length then
            match isTabular objRows with
            | Some fields ->
                // Tabular form
                let fieldStr = fields |> List.map (fun f -> if Text.RegularExpressions.Regex.IsMatch(f, @"^[A-Za-z_][\w.]*$") then f else escapeString f) |> String.concat (string delim)
                sb.AppendLine($"{pad}{keyPrefix}{lenMarker}{{{fieldStr}}}:") |> ignore
                for row in objRows do
                    let values = fields |> List.map (fun f -> row |> List.tryFind (fun (k,_) -> k = f) |> Option.map snd |> Option.defaultValue TNull |> encodeValue delim)
                    sb.AppendLine($"{pad}  {values |> String.concat (string delim)}") |> ignore
                ()
            | None ->
                encodeExpandedList sb indent delim keyPrefix lenMarker items
        else
            encodeExpandedList sb indent delim keyPrefix lenMarker items

and private encodeExpandedList (sb: StringBuilder) (indent: int) (delim: char) (keyPrefix: string) (lenMarker: string) (items: ToonValue list) : unit =
    let pad = String.replicate (indent * 2) " "
    sb.AppendLine($"{pad}{keyPrefix}{lenMarker}:") |> ignore
    for item in items do
        match item with
        | TObject ((firstKey, firstVal) :: rest) ->
            let fk = if Text.RegularExpressions.Regex.IsMatch(firstKey, @"^[A-Za-z_][\w.]*$") then firstKey else escapeString firstKey
            match firstVal with
            | TObject _ ->
                sb.AppendLine($"{pad}  - {fk}:") |> ignore
                encode sb (indent + 2) delim firstVal
                for (k2, v2) in rest do
                    let k2e = if Text.RegularExpressions.Regex.IsMatch(k2, @"^[A-Za-z_][\w.]*$") then k2 else escapeString k2
                    match v2 with
                    | TObject _ ->
                        sb.AppendLine($"{pad}    {k2e}:") |> ignore
                        encode sb (indent + 2) delim v2
                    | TArray arr ->
                        encodeArray sb (indent + 2) delim k2e arr
                    | _ ->
                        sb.AppendLine($"{pad}    {k2e}: {encodeValue delim v2}") |> ignore
            | TArray arr ->
                sb.AppendLine($"{pad}  - {fk}") |> ignore
                encodeArray sb (indent + 2) delim fk arr
            | _ ->
                sb.AppendLine($"{pad}  - {fk}: {encodeValue delim firstVal}") |> ignore
                for (k2, v2) in rest do
                    let k2e = if Text.RegularExpressions.Regex.IsMatch(k2, @"^[A-Za-z_][\w.]*$") then k2 else escapeString k2
                    match v2 with
                    | TObject _ ->
                        sb.AppendLine($"{pad}    {k2e}:") |> ignore
                        encode sb (indent + 2) delim v2
                    | TArray arr ->
                        encodeArray sb (indent + 2) delim k2e arr
                    | _ ->
                        sb.AppendLine($"{pad}    {k2e}: {encodeValue delim v2}") |> ignore
        | TObject [] ->
            sb.AppendLine($"{pad}  -") |> ignore
        | TArray arr ->
            let innerN = arr.Length
            let delimSuffix = if delim = '\t' then "\t" elif delim = '|' then "|" else ""
            let innerPrim = arr |> List.forall (fun i -> match i with TObject _ | TArray _ -> false | _ -> true)
            if innerPrim then
                let encoded = arr |> List.map (encodeValue delim) |> String.concat (string delim)
                sb.AppendLine($"{pad}  - [{innerN}{delimSuffix}]: {encoded}") |> ignore
            else
                sb.AppendLine($"{pad}  - [{innerN}{delimSuffix}]:") |> ignore
                encode sb (indent + 2) delim (TArray arr)
        | _ ->
            sb.AppendLine($"{pad}  - {encodeValue delim item}") |> ignore

// ─── Public API ──────────────────────────────────────────────────────────────

/// Round-trip: parse then re-serialize, yielding normalized TOON.
/// Returns Ok(normalized) or Error(message).
let serialize (input: string) : Result<string, string> =
    try
        let value = parse input
        let sb = StringBuilder()
        encode sb 0 ',' value
        // Trim trailing newline per spec §12 (no trailing newline)
        Ok (sb.ToString().TrimEnd('\n', '\r'))
    with
    | ToonParseException(lineNo, msg) ->
        Error $"Line {lineNo}: {msg}"
    | ex ->
        Error ex.Message
