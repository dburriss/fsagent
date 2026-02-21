## 1. Define custom exception type in Writers.fs

- [x] 1.0 (done) Collect-then-throw pattern implemented
- [x] 1.1 Define `exception ValidationException of string` in `AgentWriter` module in `Writers.fs`
- [x] 1.2 Replace `failwith` in `renderAgent` with `raise (ValidationException ...)`
- [x] 1.3 Replace `failwith` in `renderCommand` with `raise (ValidationException ...)`
- [x] 1.4 Replace `failwith` in `renderSkill` with `raise (ValidationException ...)`

## 2. Update tests to catch ValidationException

- [x] 2.1 Update `CommandTests.fs` — change `Assert.Throws<System.Exception>` to `Assert.Throws<AgentWriter.ValidationException>`
- [x] 2.2 Update `SkillTests.fs` — change `Assert.Throws<System.Exception>` to `Assert.Throws<AgentWriter.ValidationException>`

## 3. Verify and update changelog

- [x] 3.1 Run `dotnet build` — verify zero errors
- [x] 3.2 Run `dotnet test` — verify all tests pass
- [x] 3.3 Update `CHANGELOG.md` entry to mention `ValidationException`
