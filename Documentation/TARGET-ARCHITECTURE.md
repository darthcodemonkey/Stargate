# Target Architecture

## Project Structure

```
exercise1/
├── StargateAPI.sln
├── api/
│   ├── StargateAPI.csproj
│   ├── Program.cs
│   ├── Controllers/
│   │   ├── PersonController.cs
│   │   └── AstronautDutyController.cs
│   ├── DTOs/
│   │   ├── CreatePersonDto.cs
│   │   ├── UpdatePersonDto.cs
│   │   ├── PersonDto.cs
│   │   ├── CreateAstronautDutyDto.cs
│   │   └── AstronautDutyDto.cs
│   └── appsettings.json
├── domain/
│   ├── StargateAPI.Domain.csproj
│   ├── Models/
│   │   ├── Person.cs
│   │   ├── AstronautDuty.cs
│   │   └── AstronautDetail.cs
│   ├── Data/
│   │   ├── StargateContext.cs
│   │   └── Configurations/
│   │       ├── PersonConfiguration.cs
│   │       ├── AstronautDutyConfiguration.cs
│   │       └── AstronautDetailConfiguration.cs
│   ├── Repositories/
│   │   ├── PersonRepository.cs
│   │   ├── AstronautDutyRepository.cs
│   │   └── AstronautDetailRepository.cs
│   └── Interfaces/
│       ├── IPersonRepository.cs
│       ├── IAstronautDutyRepository.cs
│       ├── IAstronautDetailRepository.cs
│       ├── IPersonService.cs
│       └── IAstronautDutyService.cs
├── services/
│   ├── StargateAPI.Services.csproj
│   └── Services/
│       ├── PersonService.cs
│       └── AstronautDutyService.cs
└── tests/
    └── StargateAPI.Tests/
        ├── StargateAPI.Tests.csproj
        ├── Services/
        │   ├── PersonServiceTests.cs
        │   └── AstronautDutyServiceTests.cs
        └── Repositories/
            ├── PersonRepositoryTests.cs
            └── AstronautDutyRepositoryTests.cs
```

## Layer Responsibilities

### API Layer (`api/`)
**Responsibilities**:
- HTTP request/response handling
- Route mapping
- Input validation (basic)
- Status code mapping
- DTO mapping (API ↔ Domain)
- No business logic
- No direct database access

**Dependencies**: Services, Domain (interfaces only)

### Services Layer (`services/`)
**Responsibilities**:
- Business logic orchestration
- Business rule enforcement
- Transaction management
- Cross-cutting concerns (logging, validation)
- Coordinate between repositories

**Dependencies**: Domain (interfaces only)

### Domain Layer (`domain/`)
**Responsibilities**:
- Domain models (entities, value objects)
- Repository interfaces
- Service interfaces
- Repository implementations
- DbContext and EF configurations
- Domain validation

**Dependencies**: None (pure domain)

## Data Flow

```
HTTP Request
    ↓
Controller (API Layer)
    ↓
Service (Services Layer)
    ↓
Repository (Domain Layer)
    ↓
DbContext → SQL Server
```

## Dependency Direction

```
API → Services → Domain
```

All dependencies point inward toward the domain.

## Technology Stack

- **.NET 8.0**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **SQL Server Developer Edition** (localhost)
- **Serilog** (structured logging)
- **xUnit** (testing)

## API Endpoints

### Person Endpoints
- `GET /api/person` - Get all people
- `GET /api/person/{name}` - Get person by name
- `POST /api/person` - Create person
- `PUT /api/person/{name}` - Update person

### Astronaut Duty Endpoints
- `GET /api/astronaut-duty/{name}` - Get astronaut duties by name
- `POST /api/astronaut-duty` - Create astronaut duty

## Business Rules Enforcement

All business rules are enforced in the **Services Layer**:

1. Person uniqueness (Name)
2. One current duty at a time
3. Current duty has no end date
4. Previous duty end date calculation
5. Retired status logic
6. Career end date calculation

## Logging Strategy

- **Startup**: Application initialization, service registration, database connection
- **Services**: Method entry/exit, business rule enforcement, operations
- **Repositories**: Database queries, saves, errors
- **Controllers**: Request received, response sent, errors

All logging uses Serilog with structured logging.

