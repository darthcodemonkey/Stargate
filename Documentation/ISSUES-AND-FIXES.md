# Issues and Fixes Summary

## Critical Issues

### 1. SQL Injection Vulnerabilities
**Severity**: Critical  
**Location**: Multiple handler files using raw SQL with string interpolation

**Affected Files**:
- `CreateAstronautDutyHandler.cs`
- `GetPeopleHandler.cs`
- `GetPersonByNameHandler.cs`
- `GetAstronautDutiesByNameHandler.cs`

**Fix**: Replace all raw SQL with EF Core LINQ queries

---

### 2. Controller Bug
**Severity**: High  
**File**: `AstronautDutyController.cs:24`

**Issue**: Uses `GetPersonByName` instead of `GetAstronautDutiesByName`

**Fix**: Change to correct handler/query

---

### 3. Missing Endpoint
**Severity**: High  
**Requirement**: "Add/update a person by name"

**Issue**: Only POST (create) exists, PUT (update) is missing

**Fix**: Add PUT endpoint `/api/person/{name}`

---

## Architecture Issues

### 4. No Clean Architecture
**Issue**: All code in single project  
**Fix**: Split into domain, services, and api projects

### 5. No DDD Separation
**Issue**: Domain, services, and API mixed together  
**Fix**: Proper layer separation per PROJECT-RULES.md

### 6. Business Logic in Handlers
**Issue**: Complex business rules in MediatR handlers  
**Fix**: Move to service layer

---

## Technology Stack Issues

### 7. Wrong Database
**Current**: SQLite  
**Required**: SQL Server Developer Edition  
**Fix**: Update connection string and EF configuration

### 8. No Structured Logging
**Current**: Basic .NET logging  
**Required**: Serilog  
**Fix**: Install and configure Serilog throughout

---

## Code Quality Issues

### 9. Poor Error Handling
**Issue**: Generic try-catch, no structured errors  
**Fix**: Implement structured error responses and proper exception handling

### 10. Mixed Data Access
**Issue**: EF Core and Dapper mixed, inconsistent patterns  
**Fix**: Use EF Core exclusively with LINQ queries

---

## Business Rules Status

✅ Rule 1: Person uniquely identified by Name - Partially enforced  
✅ Rule 2: Current Duty has no DutyEndDate - Enforced  
✅ Rule 3: Previous Duty EndDate logic - Enforced  
✅ Rule 4: Retired logic - Enforced  
✅ Rule 5: CareerEndDate logic - Enforced  

**Note**: All rules should be centralized in service layer

---

## UI Integration Issues (Fixed)

### 11. Route Mismatch Between Angular and API
**Severity**: High  
**Date**: 2025-12-17

**Issue**: Angular service was calling `/api/astronaut-duty` (kebab-case) but controller route `api/[controller]` resolves to `/api/AstronautDuty` (PascalCase). ASP.NET Core routing does not automatically convert hyphens.

**Fix**: Updated `AstronautDutyService` to use `/api/AstronautDuty` to match the controller route.

**File**: `Stargate.UI/stargate.ui.client/src/app/services/astronaut-duty.service.ts`

---

### 12. API Proxy Configuration Issues
**Severity**: Medium  
**Date**: 2025-12-17

**Issue**: Proxy middleware was not correctly resolving the API service URL from Aspire service discovery, causing requests to fail with 404 or wrong port errors.

**Fix**: 
- Implemented multi-method service discovery:
  1. Try HttpClient BaseAddress from Aspire service reference
  2. Try configuration keys (`services__stargate-api__https__0`, etc.)
  3. Fallback to default port with warning
- Improved POST/PUT body handling (removed ContentLength check that could fail)
- Added comprehensive logging for debugging
- Configured HttpClient with service name `"stargate-api"` to match AppHost reference

**File**: `Stargate.UI.Server/Program.cs`

---

### 13. Static File Serving 404 Errors
**Severity**: High  
**Date**: 2025-12-17

**Issue**: Angular app returning 404 when launched from Aspire, indicating static files were not being found.

**Fix**:
- Created robust path resolution helper (`GetAngularClientDistPath`) that tries multiple strategies:
  - From ContentRootPath
  - From Assembly location
  - From current working directory
- Added extensive logging to show which path is being used
- Configured `UseStaticFiles` and `UseDefaultFiles` correctly
- Moved Swagger UI to `/swagger` route to prevent conflicts
- Added automatic Angular build via MSBuild JavaScript SDK

**File**: `Stargate.UI.Server/Program.cs`

---

### 14. Database Migration Conflicts
**Severity**: Medium  
**Date**: 2025-12-17

**Issue**: `SqlException: 'There is already an object named 'Person' in the database'` when tables exist but migration history is out of sync.

**Fix**: Added error handling in `Program.cs` to catch `SqlException` during migrations. If error indicates object already exists, log warning and continue (allowing seeding to proceed).

**File**: `Stargate.Api/Program.cs`

---

### 15. Missing Automatic Angular Builds
**Severity**: Medium  
**Date**: 2025-12-17

**Issue**: Had to manually run `ng build` when launching from Visual Studio.

**Fix**: Configured `Stargate.UI.Client.esproj` with `<ShouldRunBuildScript>true</ShouldRunBuildScript>` to automatically run `npm run build` during .NET build process.

**File**: `Stargate.UI/stargate.ui.client/Stargate.UI.Client.esproj`

