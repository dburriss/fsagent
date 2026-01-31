# Fue - F# Templating Library

## Overview

Fue is a simple F# templating library designed to work natively with F# types. It eliminates the need for ViewModels by allowing direct use of F# data structures like discriminated unions, tuples, records, and options in templates.

**Repository**: https://github.com/Dzoukr/Fue

## Installation

```bash
# NuGet
dotnet add package Fue

# Or via Paket
nuget Fue
```

## Core Concepts

### Template Syntax

- **Variable Interpolation**: `{{{value}}}` - renders the value
- **Control Flow**: HTML-like attributes (`fs-if`, `fs-else`, `fs-for`, etc.)
- **Default Values**: `{{{value ?? "default"}}}` - provides fallback

### API Pattern

1. Initialize data storage with `init`
2. Add variables with `add`
3. Render with `fromText` or `fromFile`

```fsharp
open Fue.Data
open Fue.Compiler

init
|> add "name" "Roman"
|> fromText "<div>{{{name}}}</div>"
// Result: "<div>Roman</div>"
```

## Basic Usage

### Simple Variable Rendering

```fsharp
let html = """
<h1>{{{title}}}</h1>
<p>{{{description ?? "No description available"}}}</p>
"""

init
|> add "title" "Welcome"
|> fromText html
```

### From File Templates

```fsharp
init
|> add "name" "John"
|> add "age" 30
|> fromFile "template.html"
```

### Safe Rendering (HTML Encoding)

Use `fromTextSafe` or `fromFileSafe` to automatically HTML-encode values and prevent XSS:

```fsharp
init
|> add "userInput" "<script>alert('XSS')</script>"
|> fromTextSafe "<div>{{{userInput}}}</div>"
// Result: "<div>&lt;script&gt;alert('XSS')&lt;/script&gt;</div>"
```

## Working with F# Types

### Records

```fsharp
type Person = { Name: string; Age: int }

let template = """
<div>
  <h2>{{{person.Name}}}</h2>
  <p>Age: {{{person.Age}}}</p>
</div>
"""

init
|> add "person" { Name = "Alice"; Age = 25 }
|> fromText template
```

### Options

```fsharp
let template = """
<div fs-if="user.IsSome">
  Welcome, {{{user.Value.Name}}}!
</div>
<div fs-if="user.IsNone">
  Please log in.
</div>
"""

init
|> add "user" (Some { Name = "Bob" })
|> fromText template
```

### Discriminated Unions

```fsharp
type Status =
    | Active of days: int
    | Inactive
    | Pending of reason: string

let template = """
<div fs-du="status">
  <span fs-case="Active">Active for {{{days}}} days</span>
  <span fs-case="Inactive">Currently inactive</span>
  <span fs-case="Pending">Pending: {{{reason}}}</span>
</div>
"""

init
|> add "status" (Active 30)
|> fromText template
```

## Control Flow

### Conditionals

```fsharp
let template = """
<div fs-if="isLoggedIn">
  <p>Welcome back!</p>
</div>
<div fs-else>
  <p>Please log in</p>
</div>
"""

init
|> add "isLoggedIn" true
|> fromText template
```

### Loops

```fsharp
let template = """
<ul>
  <li fs-for="item in items">
    {{{$iteration}}}. {{{item}}}
    <span fs-if="$is-last">(last)</span>
  </li>
</ul>
"""

init
|> add "items" ["Apple"; "Banana"; "Cherry"]
|> fromText template
```

**Loop Context Variables:**
- `$index` - zero-based index
- `$iteration` - one-based iteration number
- `$length` - total number of items
- `$is-last` - true for the last item
- `$is-not-last` - true for non-last items

### Tuple Destructuring in Loops

```fsharp
let template = """
<div fs-for="key,value in pairs">
  {{{key}}}: {{{value}}}
</div>
"""

init
|> add "pairs" [("Name", "Alice"); ("Age", "30")]
|> fromText template
```

## Functions in Templates

```fsharp
let toUpper (s: string) = s.ToUpper()

let template = "{{{name | toUpper}}}"

init
|> add "name" "alice"
|> add "toUpper" toUpper
|> fromText template
// Result: "ALICE"
```

## Advanced Features

### Template Placeholders

The `<fs-template>` element renders only its inner content (useful with conditionals):

```fsharp
let template = """
<fs-template fs-if="showMessage">
  <p>Message 1</p>
  <p>Message 2</p>
</fs-template>
"""
// Renders both paragraphs without wrapper element
```

### Nested Records and Piping

```fsharp
type Address = { City: string; Country: string }
type User = { Name: string; Address: Address }

let template = """
<p>{{{user.Name | toUpper}}}</p>
<p>{{{user.Address.City}}}, {{{user.Address.Country}}}</p>
"""

init
|> add "user" { Name = "John"; Address = { City = "Prague"; Country = "CZ" } }
|> add "toUpper" (fun (s: string) -> s.ToUpper())
|> fromText template
```

### Record Literals in Templates

```fsharp
let template = """
{{{ { Name = name; Age = age } | formatPerson }}}
"""

init
|> add "name" "Alice"
|> add "age" 25
|> add "formatPerson" (fun p -> sprintf "%s (%d)" p.Name p.Age)
|> fromText template
```

## Key Advantages

1. **Type Safety**: Works directly with F# types without transformation
2. **No ViewModels**: Use domain types directly in templates
3. **Minimal API**: Simple, focused surface area
4. **Security**: Built-in XSS protection with Safe variants
5. **Composable**: Supports piping and function composition
6. **Native DU Support**: Pattern matching in templates

## Common Patterns

### Building Complex Data

```fsharp
init
|> add "title" "My Page"
|> add "users" [
    { Name = "Alice"; Age = 25 }
    { Name = "Bob"; Age = 30 }
]
|> add "config" { Theme = "dark"; Language = "en" }
|> fromFile "page.html"
```

### Conditional Content with Options

```fsharp
let template = """
<div fs-if="error.IsSome" class="error">
  Error: {{{error.Value}}}
</div>
"""

init
|> add "error" (Some "Invalid input")
|> fromText template
```

### Processing Lists with Context

```fsharp
let template = """
<div fs-for="item in items">
  <span fs-if="$is-not-last">{{{item}}}, </span>
  <span fs-if="$is-last">{{{item}}}</span>
</div>
"""

init
|> add "items" ["Red"; "Green"; "Blue"]
|> fromText template
// Result: Red, Green, Blue
```

## Summary

Fue provides a minimalist, F#-friendly templating solution that respects the type system and eliminates the need for mapping layers. It's ideal for generating HTML, configuration files, or any text-based output where F# types should be used directly.
