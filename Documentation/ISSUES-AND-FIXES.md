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

