## 1. Update renderAgent validation in Writers.fs

- [ ] 1.1 Replace the single-condition `if name.IsNone || desc.IsNone` guard with a collect-then-throw pattern that reports each missing field separately
- [ ] 1.2 Verify the error message header reads `Agent (Copilot) validation failed:` and each violation is prefixed with `- `

## 2. Add renderCommand validation in Writers.fs

- [ ] 2.1 Insert a collect-then-throw validation block at the start of `renderCommand`, before `agentLike` construction
- [ ] 2.2 Check `cmd.Name` with `System.String.IsNullOrWhiteSpace`; add `"requires a non-empty 'name'"` to errors if blank
- [ ] 2.3 Check `cmd.Description` with `System.String.IsNullOrWhiteSpace`; add `"requires a non-empty 'description'"` to errors if blank
- [ ] 2.4 Raise `failwith ("SlashCommand validation failed:\n" + bulleted errors)` when errors list is non-empty

## 3. Add renderSkill validation in Writers.fs

- [ ] 3.1 Insert a collect-then-throw validation block at the start of `renderSkill`, after `defaultOptions()` call and before frontmatter emission
- [ ] 3.2 Extract `skillName` via `skill.Frontmatter.TryFind "name" |> Option.map string`
- [ ] 3.3 Extract `skillDesc` via `skill.Frontmatter.TryFind "description" |> Option.map string`
- [ ] 3.4 Use `Option.forall System.String.IsNullOrWhiteSpace` for both; add appropriate error strings when true
- [ ] 3.5 Raise `failwith ("Skill validation failed:\n" + bulleted errors)` when errors list is non-empty
- [ ] 3.6 Add a comment noting that `Option.forall p None = true` (vacuous truth) is the intended behaviour for missing keys

## 4. Add tests to CommandTests.fs

- [ ] 4.1 Add C test `renderCommand reports all errors when name and description are both empty` — construct `{ Name = ""; Description = ""; Sections = [] }`, assert `Assert.Throws<System.Exception>`, assert message contains `'name'` and `'description'`

## 5. Add tests to SkillTests.fs

- [ ] 5.1 Add C test `renderSkill reports all errors when name and description are both missing` — construct `{ Frontmatter = Map.empty; Sections = [] }`, assert `Assert.Throws<System.Exception>`, assert message contains `'name'` and `'description'`

## 6. Verify and finalise

- [ ] 6.1 Run `dotnet build` — verify zero errors
- [ ] 6.2 Run `dotnet test` — verify all tests pass including the two new C tests
- [ ] 6.3 Add entry under `[Unreleased]` in `CHANGELOG.md` describing the collect-then-throw validation for `renderCommand`, `renderSkill`, and the updated `renderAgent` guard
