# Stargate API Refactoring Plan

## Overview
This document outlines the comprehensive refactoring plan to address critical issues, implement proper architecture patterns, and align with project rules (Clean Architecture, DDD, SQL Server, Serilog, REST APIs).

## Current State Analysis

### Current Structure
- **Single Project**: All code is in the `api` project
- **Database**: SQLite (`starbase.db`)
- **Pattern**: MediatR with commands/queries in `Business` folder
- **Logging**: Basic .NET logging, no structured logging
- **Data Access**: Mix of EF Core and Dapper with raw SQL strings

### API Endpoints Status
1. ✅ `GET /Person` - Retrieve all people
2. ✅ `GET /Person/{name}` - Retrieve person by name
3. ✅ `POST /Person` - Add person
4. ⚠️ `PUT /Person/{name}` - **MISSING** - Update person endpoint
5. ✅ `GET /AstronautDuty/{name}` - Retrieve astronaut duties by name (but has bug)
6. ✅ `POST /AstronautDuty` - Add astronaut duty

---

## Critical Issues Found

### 1. Security Vulnerabilities

#### SQL Injection Vulnerabilities
**Location**: Multiple files using raw SQL with string interpolation

- `CreateAstronautDutyHandler.cs` (lines 56, 60, 90, 92)
  ```csharp
  var query = $"SELECT * FROM [Person] WHERE \'{request.Name}\' = Name";
  ```
  
- `GetPeopleHandler.cs` (line 25)
  ```csharp
  var query = $"SELECT a.Id as PersonId...";
  ```
  
- `GetPersonByNameHandler.cs` (line 26)
  ```csharp
  var query = $"SELECT a.Id as PersonId... WHERE '{request.Name}' = a.Name";
  ```
  
- `GetAstronautDutiesByNameHandler.cs` (lines 28, 34)
  ```csharp
  var query = $"SELECT a.Id... WHERE \'{request.Name}\' = a.Name";
  ```

**Fix**: Replace all raw SQL with EF Core LINQ queries or parameterized queries.

### 2. Bugs

#### Bug #1: Wrong Query Handler in Controller
**File**: `AstronautDutyController.cs` (line 24)
- Uses `GetPersonByName` instead of `GetAstronautDutiesByName`
- **Impact**: Returns wrong data structure

#### Bug #2: Missing Update Endpoint
**Requirement**: "Add/update a person by name"
- Only POST (create) exists, no PUT (update)
- **Impact**: Cannot fulfill requirement

### 3. Architecture Violations

#### Current Issues:
- **No Clean Architecture**: All code in single project
- **No DDD Separation**: Domain, Services, and API all mixed
- **Business Logic in Handlers**: Complex logic should be in services
- **Direct DbContext Access**: Controllers/handlers access database directly
- **MediatR Overuse**: Using MediatR where simple service injection would suffice

#### Required Structure (per PROJECT-RULES.md):
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

### 4. Technology Stack Issues

#### Database
- **Current**: SQLite (`Data Source=starbase.db`)
- **Required**: SQL Server Developer Edition on `localhost`
- **Action**: Update connection string and EF configuration

#### Logging
- **Current**: Basic .NET logging
- **Required**: Serilog with structured logging
- **Action**: Install Serilog packages, configure, add logging throughout

### 5. Code Quality Issues

#### Error Handling
- Generic try-catch blocks
- No structured error responses
- Exceptions thrown as `BadHttpRequestException` with generic messages
- No logging of errors

#### Business Logic Issues
- Complex business rules in handlers (e.g., `CreateAstronautDutyHandler`)
- Rules enforcement scattered:
  - Rule: "Person uniquely identified by Name" - partially enforced
  - Rule: "Current Duty has no DutyEndDate" - enforced
  - Rule: "Previous Duty EndDate = day before new Duty StartDate" - enforced
  - Rule: "Person is 'Retired' when DutyTitle is 'RETIRED'" - enforced
  - Rule: "CareerEndDate = day before Retired Duty StartDate" - enforced (line 85)

#### Data Access Issues
- Mix of EF Core and Dapper
- Raw SQL strings (security risk)
- Inconsistent query patterns

---

## Architecture Refactoring Plan

### Phase 1: Create Project Structure

1. **Create Domain Project** (`StargateAPI.Domain`)
   - Move domain models: `Person`, `AstronautDuty`, `AstronautDetail`
   - Move `StargateContext` to `/Data`
   - Move Entity Framework configurations
   - Create `/Interfaces` folder
   - Create repository interfaces: `IPersonRepository`, `IAstronautDutyRepository`, `IAstronautDetailRepository`
   - Create service interfaces: `IPersonService`, `IAstronautDutyService`

2. **Create Services Project** (`StargateAPI.Services`)
   - Create service implementations
   - Reference Domain project
   - Move business logic from handlers to services

3. **Refactor API Project** (`StargateAPI` or keep current name)
   - Keep only controllers/endpoints
   - Reference Domain and Services projects
   - Remove MediatR (or keep minimal if needed)
   - Add DTOs folder for API-specific DTOs

4. **Create Tests Project** (`StargateAPI.Tests`)
   - Unit tests for services
   - Integration tests for API
   - Repository tests

### Phase 2: Implement Domain Layer

#### Domain Models
- Move to `domain/Models/`
- Ensure proper domain modeling (value objects if needed)
- Add domain validation

#### Repository Interfaces (domain/Interfaces/)
```csharp
public interface IPersonRepository
{
    Task<Person?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Person>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Person> CreateAsync(Person person, CancellationToken cancellationToken = default);
    Task<Person> UpdateAsync(Person person, CancellationToken cancellationToken = default);
}

public interface IAstronautDutyRepository
{
    Task<IEnumerable<AstronautDuty>> GetByPersonNameAsync(string name, CancellationToken cancellationToken = default);
    Task<AstronautDuty?> GetCurrentDutyByPersonIdAsync(int personId, CancellationToken cancellationToken = default);
    Task<AstronautDuty> CreateAsync(AstronautDuty duty, CancellationToken cancellationToken = default);
    Task<AstronautDuty> UpdateAsync(AstronautDuty duty, CancellationToken cancellationToken = default);
}

public interface IAstronautDetailRepository
{
    Task<AstronautDetail?> GetByPersonIdAsync(int personId, CancellationToken cancellationToken = default);
    Task<AstronautDetail> CreateAsync(AstronautDetail detail, CancellationToken cancellationToken = default);
    Task<AstronautDetail> UpdateAsync(AstronautDetail detail, CancellationToken cancellationToken = default);
}
```

#### Repository Implementations (domain/Repositories/)
- Use EF Core exclusively (remove Dapper)
- All queries use LINQ (no raw SQL)
- Proper async/await patterns

#### DbContext (domain/Data/)
- Move to `domain/Data/StargateContext.cs`
- Update connection string configuration

### Phase 3: Implement Services Layer

#### Service Interfaces (domain/Interfaces/)
```csharp
public interface IPersonService
{
    Task<PersonDto> GetPersonByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<PersonDto>> GetAllPeopleAsync(CancellationToken cancellationToken = default);
    Task<PersonDto> CreatePersonAsync(string name, CancellationToken cancellationToken = default);
    Task<PersonDto> UpdatePersonAsync(string name, UpdatePersonRequest request, CancellationToken cancellationToken = default);
}

public interface IAstronautDutyService
{
    Task<AstronautDutiesDto> GetAstronautDutiesByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<AstronautDutyDto> CreateAstronautDutyAsync(CreateAstronautDutyRequest request, CancellationToken cancellationToken = default);
}
```

#### Service Implementations (services/Services/)
- **PersonService**: All person-related business logic
- **AstronautDutyService**: All astronaut duty business logic
  - Enforce business rules:
    - Person uniqueness by name
    - One current duty at a time
    - Current duty has no end date
    - Previous duty end date = new duty start date - 1 day
    - Retired logic
    - Career end date logic

### Phase 4: Update API Layer

#### Controllers
- Inject services instead of MediatR
- Keep controllers thin (HTTP concerns only)
- Proper REST endpoints:
  - `GET /api/person` - Get all people
  - `GET /api/person/{name}` - Get person by name
  - `POST /api/person` - Create person
  - `PUT /api/person/{name}` - Update person (NEW)
  - `GET /api/astronaut-duty/{name}` - Get duties by name (FIX bug)
  - `POST /api/astronaut-duty` - Create duty

#### DTOs
- API-specific DTOs in `api/DTOs/`
- Separate from domain models
- Mapping between DTOs and domain models

### Phase 5: Infrastructure Updates

#### Database Migration
1. Update connection string to SQL Server:
   ```json
   "ConnectionStrings": {
     "StargateDatabase": "Server=localhost;Database=Stargate;Trusted_Connection=true;TrustServerCertificate=true;"
   }
   ```

2. Update `Program.cs`:
   ```csharp
   builder.Services.AddDbContext<StargateContext>(options => 
       options.UseSqlServer(builder.Configuration.GetConnectionString("StargateDatabase")));
   ```

3. Remove SQLite package, add SQL Server package
4. Create new migration for SQL Server

#### Serilog Configuration
1. Install packages:
   - `Serilog.AspNetCore`
   - `Serilog.Sinks.Console`
   - `Serilog.Sinks.File`
   - `Serilog.Sinks.MSSqlServer` (for database logging if required)

2. Configure in `Program.cs`:
   ```csharp
   Log.Logger = new LoggerConfiguration()
       .ReadFrom.Configuration(builder.Configuration)
       .Enrich.FromLogContext()
       .CreateLogger();
   
   builder.Host.UseSerilog();
   ```

3. Add logging throughout:
   - Startup logging (services registered, database connected)
   - Service method entry/exit
   - Business rule enforcement
   - Errors with context
   - Success operations

---

## Implementation Checklist

### Phase 1: Project Structure
- [ ] Create `domain` folder and project
- [ ] Create `services` folder and project
- [ ] Create `tests` folder and project
- [ ] Create solution file (.sln)
- [ ] Set up project references

### Phase 2: Domain Layer
- [ ] Move domain models to domain project
- [ ] Move DbContext to domain/Data
- [ ] Create repository interfaces
- [ ] Implement repositories with EF Core (no raw SQL)
- [ ] Create service interfaces
- [ ] Update Entity Framework configurations

### Phase 3: Services Layer
- [ ] Implement PersonService
- [ ] Implement AstronautDutyService
- [ ] Move business logic from handlers to services
- [ ] Enforce all business rules in services
- [ ] Add validation

### Phase 4: API Layer
- [ ] Update controllers to use services
- [ ] Remove MediatR (or minimize usage)
- [ ] Fix AstronautDutyController bug
- [ ] Add PUT endpoint for person update
- [ ] Create API DTOs
- [ ] Add DTO mapping

### Phase 5: Infrastructure
- [ ] Update to SQL Server
- [ ] Configure Serilog
- [ ] Add startup logging
- [ ] Add logging throughout application
- [ ] Improve error handling
- [ ] Add structured error responses

### Phase 6: Testing
- [ ] Add unit tests for services
- [ ] Add unit tests for repositories
- [ ] Add integration tests for API
- [ ] Ensure >50% code coverage

### Phase 7: Documentation
- [ ] Update API documentation
- [ ] Document business rules
- [ ] Document architecture decisions

---

## Business Rules Enforcement

All rules must be enforced in service layer:

1. **Person uniquely identified by Name** - Enforce in CreatePerson and UpdatePerson
2. **Person without astronaut assignment has no Astronaut records** - Enforce in queries
3. **One current Astronaut Duty at a time** - Enforce in CreateAstronautDuty
4. **Current Duty has no DutyEndDate** - Enforce in CreateAstronautDuty
5. **Previous Duty EndDate = day before new Duty StartDate** - Enforce in CreateAstronautDuty
6. **Person is 'Retired' when DutyTitle is 'RETIRED'** - Enforce in CreateAstronautDuty
7. **CareerEndDate = day before Retired Duty StartDate** - Enforce in CreateAstronautDuty

---

## Security Fixes

### SQL Injection Prevention
- Remove all raw SQL strings
- Use EF Core LINQ exclusively
- If Dapper must be used, use parameterized queries only

### Input Validation
- Validate all inputs in controllers or using FluentValidation
- Sanitize string inputs
- Validate date ranges

---

## Logging Strategy

### Startup Logging
- Application name and version
- Environment (Development/Production)
- Database connection status
- Services registered
- Configuration flags

### Feature-Level Logging
- Service method entry with parameters
- Business rule enforcement
- Database operations (queries, saves)
- Success operations with results
- Errors with full context (exception, stack trace, input parameters)

### Log Levels
- **Debug**: Detailed diagnostic information
- **Information**: General flow, successful operations
- **Warning**: Unexpected situations that don't stop execution
- **Error**: Exceptions and failures

---

## Testing Strategy

### Unit Tests
- Test services with mocked repositories
- Test business rule enforcement
- Test error scenarios
- Target >50% code coverage

### Integration Tests
- Test API endpoints end-to-end
- Test database operations
- Test error handling

---

## Migration Path

1. **Phase 1-2**: Create new structure alongside existing code
2. **Phase 3**: Move business logic to services
3. **Phase 4**: Update controllers to use services
4. **Phase 5**: Update infrastructure (database, logging)
5. **Phase 6**: Remove old code (MediatR handlers, raw SQL)
6. **Phase 7**: Testing and documentation

---

## Risk Mitigation

- Maintain backward compatibility during migration where possible
- Create feature flags if needed
- Test each phase before moving to next
- Keep existing functionality working throughout refactoring
- Use version control branches for major changes

---

## Estimated Impact

- **Security**: Critical (SQL injection fixes)
- **Architecture**: Major (complete restructure)
- **Functionality**: Minor additions (PUT endpoint, bug fix)
- **Code Quality**: Significant improvement
- **Maintainability**: Significant improvement

---

## References

- PROJECT-RULES.md - Project architecture and coding standards
- README.md - Requirements and business rules

