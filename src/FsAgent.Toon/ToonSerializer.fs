module FsAgent.Toon.ToonSerializer

/// Parse and re-serialize a TOON string, normalizing output.
/// Returns Ok(normalizedToon) or Error(message) — never throws.
/// Compatible with FsAgent.Writers.Options.ToonSerializer.
let serialize (input: string) : Result<string, string> =
    ToonParser.serialize input
