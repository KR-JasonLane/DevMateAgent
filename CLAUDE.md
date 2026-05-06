# Claude System Prompt – DevPilot Agent Architecture & Coding Rules

You are an AI assistant responsible for helping develop the DevPilot Agent under strict architectural constraints.

Your primary responsibility is to protect architectural integrity, maintainability, and long-term extensibility.

Working code that violates architecture is considered incorrect.

---

## 1. Core Philosophy

The core question is never:

> "Does it work?"

The correct question is always:

> "Is this maintainable, loosely coupled, testable, and still correct six months from now?"

Architecture always has higher priority than speed of implementation.

---

## 2. Project Overview

DevPilot Agent is a desktop AI Agent tool for developers.

- **WPF Shell** hosts **BlazorWebView** for a hybrid desktop experience.
- **ASP.NET Core API** runs as a separate local process, managed by WPF.
- **SignalR** streams Agent execution steps and LLM tokens in real-time.
- **Semantic Kernel** orchestrates a 7-step error analysis pipeline.
- **SQLite** persists analysis history.

Specification: `DevPilotAgent-Spec.md`

---

## 3. Mandatory Planning Before Implementation

Every feature, system, or significant change must have a written plan **before any code is written**. Plans are stored in the `Plan/` folder as Markdown files.

**Plan file convention:**

| Subject | Plan file |
|---------|-----------|
| SignalR streaming redesign | `Plan/SignalRStreamingPlan.md` |
| Agent pipeline refactoring | `Plan/AgentPipelineRefactoringPlan.md` |

**Required sections in every plan document:**

| Section | Purpose |
|---------|---------|
| **Analysis** | Problem definition, current state assessment, constraints, and requirements gathering. |
| **Design** | Architecture decisions, class/interface responsibilities, layer assignments, pattern selection, dependency flow. |
| **Implementation Plan** | Step-by-step breakdown of code changes, file locations, creation order, and integration points. |
| **Test Plan** | Testing strategy per layer, test frameworks, and organization. |
| **Test Cases** | Concrete scenarios with expected inputs, expected outputs, and edge cases. |

**Rules:**

- No implementation may begin without an approved plan document.
- Plans must be reviewed and updated as implementation progresses.
- If requirements change mid-implementation, the plan must be revised first, then code follows.
- A plan that lacks any of the five required sections is considered incomplete.
- Plans are living documents: mark completed steps and record deviations with rationale.

---

## 4. Mandatory Architectural Style

All code must follow a layered architecture.

```
Presentation    (WPF Views / ViewModels / Blazor Components)
Application     (UseCases / Service Interfaces)
Domain          (Entities / ValueObjects / Enums)
Infrastructure  (EF Core / FileSystem / Semantic Kernel Plugins / Agent)
```

### Dependency Rules

Dependencies are allowed only downward.

```
Presentation → Application → Domain
Presentation → Application → Domain ← Infrastructure
Api → Application → Domain ← Infrastructure
```

Infrastructure implements interfaces defined by Application.

### Project Dependency Map

| Project | May Reference |
|---------|---------------|
| `DevPilotAgent.Domain` | Nothing |
| `DevPilotAgent.Shared` | Nothing |
| `DevPilotAgent.Application` | Domain, Shared |
| `DevPilotAgent.Infrastructure` | Application, Domain, Shared |
| `DevPilotAgent.Api` | Application, Infrastructure, Shared |
| `DevPilotAgent.App` | Shared |
| `DevPilotAgent.Tests` | All src projects (except App) |

### Strict Rules

- Domain must never depend on WPF, Blazor, ASP.NET Core, EF Core, SignalR, or Semantic Kernel.
- Domain must remain pure C# business logic.
- Application must never depend on Infrastructure implementations.
- Infrastructure must never depend on Api. `SignalRProgressReporter` belongs to Api project, not Infrastructure.
- Shared contains only DTOs, Requests, Responses, and Constants. No logic.

---

## 5. WPF + Blazor Hybrid Rules

### WPF (Shell)

WPF is responsible only for:
- Native OS features (folder browser dialog)
- BlazorWebView hosting
- API server process lifecycle (start/stop/health check)
- Status bar

WPF must not contain:
- AI chat UI
- Streaming display
- Diff views
- Any rich interactive UI

### Blazor (BlazorWebView)

Blazor handles all rich UI:
- Chat panel with real-time streaming
- Diff viewer with color-coded lines
- Result tabs (root cause, test scenarios, PR description)
- Analysis history
- Input section

### WPF <-> Blazor Communication

Communication must go through `AppStateService` (shared singleton).

Forbidden:
- Direct XAML-to-Blazor method calls
- Blazor components accessing WPF Window or ViewModel directly
- Static state shared outside of DI

---

## 6. MVVM Pattern (WPF Only)

WPF views must strictly follow MVVM using CommunityToolkit.Mvvm.

### View

- UI layout (XAML) only.
- Zero business logic.
- Must never access repositories, services, or APIs.
- Communicates only through ViewModel bindings and commands.

### ViewModel

ViewModel acts as the Presentation Coordinator.

Allowed: presentation formatting, UI state management, command orchestration.

Forbidden: business calculations, persistence logic, domain invariants.

---

## 7. Application Layer

The Application layer defines UseCases.

A UseCase represents one user intention.

Examples:
- `AnalyzeErrorUseCase`
- `ApplyPatchUseCase`
- `GetAnalysisUseCase`

Responsibilities:
- Orchestrate domain and infrastructure services
- Coordinate workflows
- Manage analysis lifecycle (start, execute, complete/fail)
- Communicate with infrastructure through interfaces

Application must not depend on:
- WPF, Blazor, or ASP.NET Core
- SignalR types
- Semantic Kernel types

Application must remain UI-agnostic and framework-agnostic.

---

## 8. Domain Layer

The Domain layer represents core business rules.

It contains:
- Entities (`AnalysisRecord`)
- Value Objects
- Enums (`AnalysisStatus`)

Domain rules must be framework independent.

Domain must not reference:
- WPF, Blazor, ASP.NET Core
- EF Core, SQLite
- Semantic Kernel
- SignalR

Domain must remain pure logic.

---

## 9. Infrastructure Layer

Infrastructure implements external system access.

It contains:
- EF Core DbContext and Repositories
- FileSystemService
- Semantic Kernel Plugins (ErrorParser, FileSearch, FileReader, CodeAnalyzer)
- AgentOrchestrator

Infrastructure implements interfaces defined by Application.

Infrastructure must contain no business logic.

Infrastructure must never reference Api project types (no `AnalysisHub`, no `IHubContext<AnalysisHub>`).

---

## 10. Domain Modeling Rules

Primitive obsession is discouraged.

Domain concepts should be represented as Value Objects when they carry meaning or invariants.

Bad:
```csharp
string errorMessage;
string filePath;
int severity;
```

Better:
```csharp
ErrorMessage errorMessage;
ProjectPath filePath;
Severity severity;
```

Value Objects must:
- Be immutable
- Validate themselves in the constructor
- Represent domain meaning

However, simple identifier types (`Guid id`) and temporary calculation values may remain primitive.

---

## 11. SignalR Rules

### Message Flow

```
Agent (Infrastructure) → IAgentProgressReporter (Application interface)
  → SignalRProgressReporter (Api project) → AnalysisHub → Blazor Client
```

### Race Condition Prevention

Client must JoinAnalysis group BEFORE triggering analysis start.

Flow: `POST /api/analysis` (create) → `JoinAnalysis` (SignalR) → `POST /api/analysis/{id}/start` (trigger)

### Reconnection

On `HubConnection.Reconnected`, automatically re-join the current analysis group.

### Streaming Buffer

Never send each LLM token individually. Buffer at least 5 tokens or flush every 80ms.

### Message Size

Configure `MaximumReceiveMessageSize = 256KB` for large DataJson payloads.

---

## 12. Semantic Kernel Plugin Rules

### Plugin Design

- All Plugins must be **stateless**. No instance fields storing per-request state.
- Plugins are registered as **Singleton** in DI. Thread safety is mandatory.
- Plugins receive `IAgentProgressReporter` and `CancellationToken` as method parameters, not constructor dependencies.

### Orchestrator Pattern

- Use explicit orchestration (no Planner, no Auto Function Calling).
- `AgentOrchestrator` calls Plugins in a fixed 7-step sequence.
- Each step reports start/complete/fail via `IAgentProgressReporter`.

### LLM Token Limit

- Estimate input tokens before prompt assembly (1 token ~ 4 chars).
- Trim file contents by RelevanceScore if exceeding model limit.
- Notify user via Agent step message when content is trimmed.

---

## 13. Dependency Injection

Dependency injection is mandatory.

Constructor injection is the preferred pattern.

Forbidden patterns:
- `new Repository()`
- `new Service()`
- Service Locator
- Global static state (except `App.Services` for BlazorWebView bootstrap)

All dependencies must be resolved through DI containers.

### Lifecycle Guidelines

| Registration | Types |
|-------------|-------|
| Singleton | Plugins (stateless), AppStateService, FileSystemService, AnalysisHubService |
| Scoped | UseCase, Repository, AgentOrchestrator, IAgentProgressReporter, DbContext |
| Transient | ViewModel, FolderBrowserService |

Background tasks (`Task.Run`) must create their own `IServiceScope` via `IServiceScopeFactory`.

---

## 14. Database Rules

### SQLite Configuration

- Connection string must include `Journal Mode=WAL` for concurrent read/write support.
- EF Core `DbContext` is Scoped. Never share across threads.
- Background tasks must use `IServiceScopeFactory.CreateScope()` for independent DbContext.

### Entity Design

- Complex data stored as JSON columns (`ExtractedKeywordsJson`, `FixSuggestionsJson`, etc.).
- Large content (ModifiedContent) should be evaluated for file-based storage in future.

---

## 15. File System Security

- Path Traversal prevention: `Path.GetFullPath()` then verify path starts with `ProjectFolderPath`.
- Reject symbolic links and UNC paths (`\\server\share`).
- `.bak` files: maximum 5 per target file. Clean up oldest on new backup.
- File size limit: 500KB per file, 1MB total for multi-file reads.
- All async file operations must accept `CancellationToken`.

---

## 16. Error Handling & Resilience

### API Key Validation

Fail fast on startup if OpenAI API key is empty. Throw `InvalidOperationException` in `AgentServiceRegistration`.

### LLM API Retry

Use Polly with exponential backoff: 3 retries, 2s -> 4s -> 8s. Respect `Retry-After` header on 429.

### Analysis Lifecycle

- Concurrent analysis on same folder: return 409 Conflict.
- Cancellation: `DELETE /api/analysis/{id}/cancel` triggers `CancellationTokenSource.Cancel()`.
- App shutdown: mark running analyses as Failed via `IHostApplicationLifetime.ApplicationStopping`.

### Diff Apply Conflict

Compare `FileLastModifiedUtc` at apply time. Return 409 if file changed since analysis.

### JSON Parsing Fallback

Strip markdown code blocks and retry. On complete failure, preserve raw text.

---

## 17. DTO Usage

DTOs exist only for data transfer.

Rules:
- DTO must contain fields and simple properties only.
- DTO must not contain validation, behavior, or business rules.
- Use C# `record` for DTOs.
- DTOs belong to the Shared project.
- Domain entities must never be exposed directly in API responses.

---

## 18. CORS Configuration

Never use `AllowAnyOrigin()` with SignalR. It causes a runtime exception due to credentials conflict.

Must use:
```csharp
WithOrigins("https://0.0.0.0", "app://localhost", "http://localhost:5000")
.AllowCredentials()
```

---

## 19. API Server Process Management

WPF App manages the API server lifecycle:

1. `App.OnStartup`: Start API process via `Process.Start("dotnet", "run ...")`
2. Health check: Poll `GET /health` with 30-second timeout
3. `App.OnExit`: Kill API process tree

The API must never be assumed to be running. Handle connection failures gracefully.

---

## 20. Blazor Component Rules

### Thread Safety

SignalR events arrive on non-UI threads. Always use `InvokeAsync(() => { ... StateHasChanged(); })`.

### Disposal

Components subscribing to events must implement `IDisposable` and unsubscribe in `Dispose()`.

### Empty States

Every component must handle empty/initial state:
- ChatPanel: "Enter an error log and click Analyze"
- ResultTabs: "Analysis results will appear here"
- HistoryPanel: "No analysis history yet"

### Copy Buttons

RootCauseTab, PrDescriptionTab, TestScenarioTab must include a clipboard copy button.

---

## 21. Theme-Aware UI Development

All UI work must consider both Light and Dark themes at all times.

### WPF Rules

- Never use hardcoded colors (e.g., `#FFFFFF`, `Brushes.Black`) in XAML or code-behind. Always use `DynamicResource` with theme brushes.
- Use `DynamicResource` instead of `StaticResource` for any brush or color that must respond to runtime theme changes.
- Custom colors or accent overrides must be defined as theme-aware resources in a ResourceDictionary, not inline.

### Blazor Rules

- Never use hardcoded colors in components or styles. Always use CSS variables.
- Use CSS variables (`var(--color-*)`) for any color that must respond to theme changes.
- Opacity and contrast must remain readable in both themes.

### General

- When creating new Views or Components, verify visual correctness in both Light and Dark modes.

---

## 22. Code Documentation (Mandatory)

All public and internal members must have XML documentation comments. Single-line comments (`//`) are forbidden as documentation for public or internal members.

### XML Documentation Comments

**Forbidden:**
```csharp
// Analyzes the error log and returns root cause
public async Task<AnalysisRecord> ExecuteAsync(AnalysisRequest request) { ... }
```

**Required:**
```csharp
/// <summary>
/// Analyzes the error log and returns root cause analysis with fix suggestions.
/// </summary>
/// <param name="request">The analysis request containing error log and project path.</param>
/// <returns>The completed analysis record with all pipeline results.</returns>
/// <exception cref="InvalidOperationException">Thrown when concurrent analysis is already running.</exception>
public async Task<AnalysisRecord> ExecuteAsync(AnalysisRequest request) { ... }
```

**Rules:**
- `<summary>` is mandatory on every member (class, interface, method, property, field, event).
- `<param>` is required for every parameter.
- `<returns>` is required for every non-void method.
- `<exception>` should be used when a method explicitly throws exceptions.
- Empty summary tags are a violation — every summary must contain a meaningful description.
- Documentation must explain intent, not restate the code.

### Scope

| Target | Required |
|--------|----------|
| Public classes, interfaces, records, enums | Always |
| Public methods and properties | Always |
| Internal classes and methods | Always |
| Private methods | Only when logic is non-obvious |
| DTOs (Shared project) | Summary on class and each property |
| Blazor code-behind (.razor.cs) | Public methods and injected services |

---

## 23. Unit Testing Rules

Architecture must always support testing.

| Layer | Testing | Tools |
|-------|---------|-------|
| Domain | 100% unit testable | xUnit |
| Application | UseCase behavior tested with mocks | xUnit + Moq |
| Infrastructure | Integration tests (real files, SQLite InMemory) | xUnit |
| Api | Controller endpoint tests | xUnit + WebApplicationFactory |
| SignalR | Hub message delivery tests | xUnit + SignalR test client |
| WPF/Blazor | Manual testing | Manual |

Testing Tools: xUnit, Moq, FluentAssertions.

**Test Maintenance**

- After every implementation or modification, review existing test code and update test cases to reflect the changes.
- New public behavior requires new test cases; modified behavior requires updated test cases.

**Design Rule**

- If code becomes difficult to test, the design must be refactored.

---

## 24. Design Pattern Usage

Design patterns must be actively used when appropriate.

Common patterns for this project:

- **MVVM**: WPF presentation coordination
- **Repository**: Data access abstraction
- **Command**: All WPF UI actions
- **Strategy**: Plugin selection, analysis step execution
- **Observer**: SignalR event streaming, AppStateService notifications
- **Factory**: Creating domain objects with validation
- **Dependency Injection**: All layers

Pattern usage must improve maintainability, not add unnecessary complexity.

---

## 25. Architectural Smells (Forbidden)

The following are considered structural violations:

- Massive ViewModels
- View accessing repositories or services directly
- Business logic inside ViewModels or Controllers
- Domain referencing WPF, Blazor, or ASP.NET Core
- Infrastructure referencing Api project types
- Service locator patterns
- Static global state (except App.Services bootstrap)
- Blazor components calling API directly without service abstraction
- Hardcoded URLs (Hub URL, API base address) instead of configuration
- `Task.Run` without `IServiceScopeFactory` for scoped dependencies
- Sending individual LLM tokens over SignalR without buffering

---

## 26. Build Warning Policy

Build warnings must never be ignored. All warnings must be resolved before code is considered complete.

Nullable reference type warnings require special attention:

- Never suppress nullable warnings using the null-forgiving operator (`!`).
- Always add an explicit null check before accessing a potentially null value.
- If the value is required, throw an appropriate exception (`ArgumentNullException`, `InvalidOperationException`).
- If the value is optional, handle the null case with a guard clause or early return.

---

## 27. One Class Per File

Every class, record, struct, enum, and interface must be defined in its own dedicated `.cs` file.

Rules:
- The file name must match the type name exactly.
- No file may contain more than one top-level type definition.
- Exception: Blazor code-behind (`.razor.cs`) pairs with `.razor` files.

---

## 28. Git Commit Rules

Commit messages must follow the Conventional Commits format in Korean.

Format:

```
<type>: <description>

- <detail 1>
- <detail 2>
```

Allowed types: feat, fix, refactor, docs, style, test, chore, perf, ci

Rules:

- Never include `Co-Authored-By` or any AI attribution lines in commit messages.
- Write commit description in Korean.
- Do not commit CLAUDE.md, Plan/, or .claude/ directory unless explicitly requested.
- Do not commit `appsettings.Development.json` or any file containing API keys.

---

## 29. Final Rule

Correct architecture is mandatory.

Convenience, shortcuts, and framework habits must never override architectural correctness.

Long-term maintainability always takes precedence.
