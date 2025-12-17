# Database Seed Data

**Date**: 2025-12-17  
**Purpose**: Populate database with realistic test data for development and testing

---

## Overview

The `DatabaseSeeder` class provides initial test data that demonstrates all business rules and various scenarios in the Astronaut Career Tracking System.

---

## Seeded Data

### 7 People / Astronauts

1. **John Smith** - Active astronaut with 3 duties (past duties + current)
2. **Sarah Johnson** - Retired astronaut with 3 duties (ending in RETIRED)
3. **Michael Chen** - Active astronaut with 4 duties (long career)
4. **Emily Rodriguez** - New astronaut with 1 current duty
5. **David Williams** - Person without astronaut assignment (no duties)
6. **Lisa Anderson** - Early retired astronaut with 2 duties
7. **Robert Martinez** - Active astronaut with 1 long-running duty

---

## Data Scenarios

### Scenario 1: Active Astronaut with Multiple Duties
**Person**: John Smith

- **Duty 1**: Mission Specialist (2015-03-15 to 2018-06-14)
- **Duty 2**: Flight Engineer (2018-06-15 to 2022-01-31)
- **Duty 3**: Commander (2022-02-01 to present - no end date)

**Demonstrates**:
- Multiple past duties with proper end dates
- Current duty has no end date
- Previous duty end date = day before new duty start date

### Scenario 2: Retired Astronaut
**Person**: Sarah Johnson

- **Duty 1**: Pilot (2012-05-10 to 2017-11-30)
- **Duty 2**: Mission Specialist (2017-12-01 to 2023-08-14)
- **Duty 3**: RETIRED (2023-08-15 to present - no end date)

**Demonstrates**:
- RETIRED duty title classification
- Career end date should be one day before RETIRED duty start date
- Retired duty has no end date

### Scenario 3: Long Career with Multiple Duties
**Person**: Michael Chen

- **Duty 1**: Mission Specialist (2008-01-20 to 2011-03-19) - Rank: Major
- **Duty 2**: Pilot (2011-03-20 to 2015-09-29) - Rank: Lieutenant Colonel
- **Duty 3**: Flight Engineer (2015-09-30 to 2020-04-14) - Rank: Colonel
- **Duty 4**: Commander (2020-04-15 to present) - Rank: Colonel

**Demonstrates**:
- Career progression (rank increases over time)
- Multiple duty types
- Long career span (2008-2024+)

### Scenario 4: New Astronaut
**Person**: Emily Rodriguez

- **Duty 1**: Mission Specialist (2024-01-10 to present)

**Demonstrates**:
- New astronaut with single duty
- Recent start date
- Current active duty

### Scenario 5: Person Without Assignment
**Person**: David Williams

- **No duties**

**Demonstrates**:
- Business Rule: "Person without astronaut assignment has no Astronaut records"
- Person exists in system but has never served as astronaut

### Scenario 6: Early Retirement
**Person**: Lisa Anderson

- **Duty 1**: Mission Specialist (2016-07-01 to 2021-05-31)
- **Duty 2**: RETIRED (2021-06-01 to present)

**Demonstrates**:
- Early retirement scenario
- RETIRED classification

### Scenario 7: Long-Running Single Duty
**Person**: Robert Martinez

- **Duty 1**: Pilot (2019-03-01 to present)

**Demonstrates**:
- Long-running single duty
- No career progression (single duty only)

---

## Business Rules Demonstrated

✅ **Rule 1**: Person uniquely identified by Name  
✅ **Rule 2**: Person without astronaut assignment has no Astronaut records (David Williams)  
✅ **Rule 3**: Person holds one current Astronaut Duty at a time  
✅ **Rule 4**: Current Duty has no DutyEndDate  
✅ **Rule 5**: Previous Duty EndDate = day before new Duty StartDate  
✅ **Rule 6**: Person classified as 'Retired' when DutyTitle is 'RETIRED'  
✅ **Rule 7**: Career EndDate = one day before Retired Duty StartDate  

---

## Implementation

### Location
- **File**: `Stargate.Domain/Data/DatabaseSeeder.cs`
- **Method**: `SeedAsync(StargateContext context, ILogger logger)`

### Integration
Called automatically during application startup in `Program.cs` after database migrations are applied.

### Idempotency
The seeder checks if data already exists before seeding:
```csharp
if (await context.People.AnyAsync())
{
    logger.LogInformation("Database already contains data. Skipping seed.");
    return;
}
```

This ensures:
- Safe to run multiple times
- Won't duplicate data
- Can be reset by clearing database

---

## Data Statistics

- **Total People**: 7
- **Total Duties**: 18
- **Active Astronauts**: 4 (John Smith, Michael Chen, Emily Rodriguez, Robert Martinez)
- **Retired Astronauts**: 2 (Sarah Johnson, Lisa Anderson)
- **People Without Assignment**: 1 (David Williams)
- **Ranks Used**: Major, Lieutenant Colonel, Colonel, Captain
- **Duty Titles**: Mission Specialist, Pilot, Flight Engineer, Commander, RETIRED

---

## Usage

### Automatic Seeding
Seed data is automatically applied when the application starts (in Development or Production environments).

### Manual Seeding (if needed)
To manually seed or re-seed the database:

1. Clear existing data:
```sql
DELETE FROM AstronautDuties;
DELETE FROM AstronautDetails;
DELETE FROM People;
```

2. Restart the application - seeding will run automatically

### Disabling Seeding
To disable automatic seeding, comment out the seeding call in `Program.cs`:
```csharp
// await DatabaseSeeder.SeedAsync(context, logger);
```

---

## Testing

The seed data provides comprehensive test scenarios for:

- ✅ Listing all people
- ✅ Viewing individual person details
- ✅ Viewing astronaut duties
- ✅ Creating new people
- ✅ Creating new duties
- ✅ Testing business rules
- ✅ Testing UI components
- ✅ Testing API endpoints
- ✅ Testing edge cases (retired, no duties, etc.)

---

## Notes

- All dates are realistic and demonstrate proper date handling
- Duty end dates properly align with subsequent duty start dates
- Current duties have `null` end dates
- RETIRED duties have no end date but indicate retirement
- Rank progression demonstrates career advancement
- Mix of active and retired astronauts for comprehensive testing

