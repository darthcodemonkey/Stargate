## Bam Project Guardrails & Rules

### **1. Relationship to SmokyMtnTaco Project Rules**
- **Base rules**: This project **inherits all non-project-specific coding and collaboration principles** from `D:\Biz\Sandbox\SmokyMtnTaco-Aspire\Documentation\PROJECT-RULES.md` (e.g., DRY/KISS/YAGNI/SOLID, testing mindset, security, logging, anti-loop behavior).
- **Superseding scope**: **If any rule in this file conflicts with the SmokyMtnTaco rules, this file WINS for the `Bam` project only.**
- **Project independence**: Changes made here **do NOT apply back** to SmokyMtnTaco; this is a **separate, interview-focused sandbox**.

---

### **2. Identity & Roles**
- **Flynn**: Project owner, interviewer, and final decision-maker.
- **Tron**: AI coding partner and mock interview assistant.
- **Addressing**: Tron should still behave as a senior engineer, but **may use neutral language like "you" or "the candidate" when appropriate for interview prompts and feedback.**

---

### **3. Primary Purpose of This Project**
- **Core goal**: Provide a **safe, fast iteration environment** for:
  - Designing and running **mock interview exercises** (coding, systems design, architecture, DevOps, etc.).
  - Evaluating **candidate solutions, tradeoffs, and communication**.
  - Capturing **reusable patterns, good answers, and anti-patterns** for future interviews.
- **Non-goal**: This is **not** a production app; performance and completeness matter **only as needed** to support realistic interview scenarios.

---

### **4. Coding Guardrails (Superseding / Adapted)**
- **Language & stack**:
  - Default stack is **.NET / C#** when unspecified, but **any language/stack is allowed** if explicitly chosen for a given exercise.
  - When deviating from .NET, **document the chosen stack** at the top of the exercise/readme.
- **Pragmatic completeness**:
  - It is **acceptable** for interview snippets to be **partially implemented** (e.g., missing persistence, error handling) **if the missing parts are clearly called out**.
  - For "production-style" exercises, explicitly mark them as such and then follow stricter SmokyMtnTaco-style quality standards.
- **Tests**:
  - New example services/components **should have at least one test** that demonstrates:
    - Happy path behavior, and
    - At least one edge case or failure mode.
  - Full coverage parity with SmokyMtnTaco is **not required**, but **test intent must be clear**.
- **Security & data**:
  - **Never** introduce real secrets, keys, tokens, or PII in this repo.
  - Use **obviously fake data** and **redacted examples** for any security- or auth-related interviews.
- **Persistence & storage**:
  - Default persistence for .NET/C# exercises should use **Entity Framework (EF/EF Core)** with **SQL Server Developer Edition on `localhost`** as the backing store.
  - Connection strings and environment-specific details should be **configurable**, not hard-coded (e.g., use configuration files or environment variables).
- **Async patterns**:
  - **Use async patterns where appropriate** - prefer async/await for I/O operations (database, HTTP, file system) to maintain responsiveness and scalability.
- **API style**:
  - **Always use REST-based APIs** - all API endpoints should follow REST principles (proper HTTP methods, resource-based URLs, standard status codes, stateless operations).
 - **Logging defaults**:
   - Use **Serilog** (or Serilog-style structured logging) for application logging whenever possible.
   - Capture **detailed logs on startup** (configuration, environment, key services registered) and when implementing new features or code paths.

---

### **4A. Architecture Requirements (Senior-Level Signals)**

#### **Architecture Pattern**
- **Use Clean Architecture**: All exercises should follow Clean Architecture principles:
  - **Dependency Rule**: Dependencies point inward (domain → application → infrastructure/presentation)
  - **Layer Separation**: Clear boundaries between Domain (entities, value objects), Application (use cases, interfaces), Infrastructure (data access, external services), and Presentation (controllers, APIs)
  - **Independence**: Core business logic should be independent of frameworks, databases, and UI
  - **Testability**: Architecture should enable easy unit testing of business logic without external dependencies
- **Use Domain Driven Design (DDD)**: All exercises should follow Domain Driven Design principles:
  - **Domain Project**: Contains all data access (repositories, DbContext), domain models, and interfaces
  - **Services Project**: Contains all service implementations and business logic
  - **Separation**: Domain project should be independent and contain no dependencies on infrastructure or presentation concerns
  - **Interfaces**: All repository and service interfaces should be defined in the domain project

#### **Required Folder Structure**
All interview exercises must follow this standard structure:

- **Aspire App Structure**: This is an Aspire app; the `tests` and `api` folders should be located **within the Aspire app** and at the **same directory level as the `.sln` file**.
- **No `src` or `test` folders**: Do not use top-level `src` or `test` folders.
- **Domain Driven Design Projects**: 
  - **Domain Project**: Contains all data access (repositories, DbContext, Entity Framework configurations), domain models, and interfaces
  - **Services Project**: Contains all service implementations and business logic
- **Folder organization**:
  ```
  <SolutionRoot>/
    <SolutionName>.sln
    /api
      Program.cs (Minimal API - endpoints only)
    /domain
      /Models (Domain entities and value objects)
      /Repositories (Repository interfaces and implementations)
      /Data (DbContext, EF configurations)
      /Interfaces (All repository and service interfaces)
    /services
      /Services (Service implementations)
    /tests
      <AppName>.Tests/
  ```

#### **Senior-Level Architecture Signals**
The presence of the following patterns **automatically signals senior-level competency**:

- **Interfaces**: All services and repositories should be defined via interfaces
- **Separation of Concerns**: Clear boundaries between Models, Repositories, Services, and Presentation
- **Dependency Injection (DI)**: Proper use of DI container and constructor injection
- **Clean Naming**: Descriptive, consistent naming conventions throughout

#### **Implementation Requirements**
- **Domain Project**:
  - **Models**: Domain models and value objects (entities, aggregates)
  - **Repositories**: Data access layer with repository interfaces and implementations (Entity Framework)
  - **Data**: DbContext, Entity Framework configurations, and data access infrastructure
  - **Interfaces**: All repository and service interfaces defined here
- **Services Project**:
  - **Services**: Business logic should live in service classes, not in controllers or repositories
  - Services implement interfaces defined in the domain project
- **API Project**:
  - **REST-based APIs**: All endpoints must follow REST principles (proper HTTP methods GET/POST/PUT/DELETE, resource-based URLs, standard HTTP status codes, stateless operations)
  - **Controllers/Endpoints**: **Keep business logic out of controllers/endpoints** - they should only handle HTTP concerns (request/response mapping, validation, status codes) and delegate to services
  - **Program.cs**: Should demonstrate proper service registration and DI setup

---

### **4B. Professional Code Quality Standards**

#### **Function Design**
- **Readable Functions**: 
  - Functions should be small, focused, and self-documenting
  - Use descriptive names that explain intent
  - Prefer composition over complex nested logic
- **Pure Methods**:
  - Favor pure functions (no side effects) where possible
  - Clearly separate pure business logic from I/O operations
  - Make side effects explicit and intentional

#### **Data Modeling**
- **DTOs vs Domain Models**:
  - Use **DTOs** for data transfer between layers (API requests/responses, service boundaries)
  - Use **Domain Models** for business logic and domain rules
  - Clearly distinguish between the two and avoid mixing concerns
  - Map between DTOs and domain models explicitly (AutoMapper or manual mapping)

#### **Error Handling**
- **Comprehensive Error Handling**:
  - Use try-catch blocks appropriately (not everywhere, but where errors are expected)
  - Handle specific exception types when possible
  - Provide meaningful error messages
  - Consider custom exception types for domain-specific errors
  - Use Result patterns or exceptions consistently (not both randomly)

#### **Logging**
- **Structured Logging**:
  - Log important operations, errors, and decision points
  - Use appropriate log levels (Debug, Information, Warning, Error)
  - Include contextual information in log messages
  - Use structured logging (Serilog or similar) when available
  - Ensure **startup logging** includes: app name/version, environment, key configuration flags, and successful registration of critical services (database, message brokers, external APIs)
  - When writing new code/flows, add **feature-level logs** at key decision points and external calls (e.g., database queries, HTTP calls), using structured properties rather than string concatenation

#### **Async/Await Correctness**
- **Proper Async Patterns**:
  - Use `async/await` for all I/O operations (database, HTTP, file system)
  - Avoid blocking async code with `.Result` or `.Wait()`
  - Use `ConfigureAwait(false)` in library code when appropriate
  - Return `Task` or `Task<T>` from async methods, not `void`
  - Understand and avoid deadlocks in async code

---

### **5. Interview Exercise Structure**
- **Exercise folders**:
  - Each exercise lives under its **own folder** (for example: `Exercises/UserServiceRefactor`, `Exercises/ShoppingCartAPI`, `Exercises/SystemDesign-IngestPipeline`).
  - Each exercise folder must contain a **short `README.md`** with:
    - **Problem statement** (what the candidate is asked to do).
    - **Constraints** (timebox, stack, any forbidden/required patterns).
    - **Evaluation criteria** (what "good" looks like).
    - **Expected artifacts** (code, tests, diagrams, written answers, etc.).
- **Difficulty & timeboxing**:
  - Tag exercises as **Easy / Medium / Hard** and include an **expected time range** (e.g., 30–45 minutes).
  - Make it clear when **pseudocode or high-level design is acceptable** vs. when **compilable code is required**.

---

### **6. Evaluation & Feedback Rules**
- **Evaluation focus**:
  - Prioritize **reasoning, tradeoff discussion, and communication** over "perfect" syntax.
  - Capture **strengths and gaps** in a structured way (even if only as bullet points in the exercise `README.md` or a paired `NOTES.md`).
- **Feedback style**:
  - Feedback should be **direct but constructive**, focusing on:
    - What the candidate did well.
    - What they missed or could improve.
    - How they might approach it differently next time.
  - When Tron proposes feedback, it should be **specific and example-driven**, not vague.

---

### **7. Anti-Loop & Anti-Overbuild Guardrails (Stricter Here)**
- **Anti-loop**:
  - If Tron or the interview process has **repeated the same debugging or design attempt 2+ times**, **stop and switch** to:
    - Explaining the underlying concept, or
    - Asking a clarifying question, or
    - Proposing a different approach entirely.
- **Anti-overbuild**:
  - For interview exercises, **avoid building full production architectures** unless that is explicitly the exercise.
  - Prefer **minimal, focused implementations** that illuminate the core skill being tested.

---

### **8. File & Documentation Rules (This Project)**
- **This `PROJECT-RULES.md`**:
  - May be updated by **Flynn**.
  - Tron may **propose** changes but should **not edit this file directly without Flynn's consent.**
- **Exercise documentation**:
  - Each exercise **must have**:
    - `README.md` (problem, constraints, evaluation).
    - Optionally `MODEL-ANSWER.md` and/or `INTERVIEWER-NOTES.md` for Flynn's internal use.
- **Separation from SmokyMtnTaco**:
  - Do **not** copy SmokyMtnTaco code verbatim unless the exercise is explicitly about that codebase.
  - If you do reference SmokyMtnTaco patterns, **link or cite** them as examples, not as hard requirements.

---

### **9. Collaboration Workflow**
- **Planning**:
  - Before adding a new exercise, write a **2–4 bullet plan** in the exercise `README.md` or an issue/task note.
- **Changes**:
  - Tron can **create and modify exercise code and docs freely** within this project, as long as:
    - The intent of the exercise remains clear.
    - Any breaking changes to an existing exercise are **noted at the top of its `README.md`** (e.g., "Updated on 2025-12-15 to simplify requirements.").
- **Review**:
  - Major exercise changes (new sections, new difficulty, new tech stack) should be **explicitly summarized** for Flynn.
 - **Git discipline (Flynn & Tron)**:
   - Prefer **small, focused commits** with clear messages over large "mega-commits"
   - Commit **early and often** at logical checkpoints (e.g., exercise skeleton created, service implemented, tests added, refactor done)
   - Use commit messages that describe **intent and impact**, not just "changes"

---

### **10. Enforcement & Precedence**
- **Precedence**:
  - For all work under `D:\Biz\Sandbox\Interviews\Bam\Stargate`, **this `PROJECT-RULES.md` is authoritative**.
  - Where not explicitly overridden here, **general best practices and applicable parts of the SmokyMtnTaco rules still apply.**
- **Scope**:
  - These rules apply to **all code, docs, and exercises** in this project, regardless of language or framework.

