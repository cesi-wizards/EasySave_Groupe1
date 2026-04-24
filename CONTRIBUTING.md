# Development Conventions

## 1. Git & Workflow
- **Branches:**
    - `main`: Stable releases only.
    - `dev`: Main integration branch.
- **Workflow:** Always fork from `dev`. Merge via PR back to `dev`.
- **Commits:** Follow [Conventional Commits](https://www.conventionalcommits.org/) (`type: description`).
  - `feat`: New feature.
  - `fix`: Bug fix.
  - `refactor`: Code change that neither fixes a bug nor adds a feature.
  - `chore`: Maintenance / config / file cleanup.
  - `docs`: Documentation only changes (README, diagrams).
- **Branch Naming:** Use the format `type/short-description` (kebab-case).
    - Examples: `feat/user-login`, `docs/diagrams`.

## 2. Releases
Follow `Major.Minor.Patch` [Semantic Versioning](https://semver.org/).
- **Major**: Breaking changes.
- **Minor**: New features, backward-compatible .
- **Patch**: Bug fixes, backward-compatible .

## 3. C# Conventions
Follow [Microsoft Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).
- **PascalCase**: Classes, Methods, Properties (`UserService`, `Execute()`).
- **camelCase**: Local variables, parameters (`orderId`).
- **_camelCase**: Private fields (`_logger`).
- **IPascalCase**: Interfaces (`IUserRepository`).

**Self-documenting code**: Code should explain the *how*, comments should explain the *why*.

## 4. Core Principles
- **KISS**: Keep It Simple. Avoid over-engineering.
- **DRY**: Don't Repeat Yourself. Refactor duplicate logic.
- **Explicit Naming**: No abbreviations (e.g., `customerIndex` instead of `ci`).
- **Error Handling**: Never leave a `catch` block empty.