using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StargateAPI.Domain.Models;

namespace StargateAPI.Domain.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(StargateContext context, ILogger logger)
    {
        try
        {
            // Check if database is already seeded
            if (await context.People.AnyAsync())
            {
                logger.LogInformation("Database already contains data. Skipping seed.");
                return;
            }

            logger.LogInformation("Starting database seed...");

            var people = new List<Person>();
            var duties = new List<AstronautDuty>();

            // Sample Astronaut 1: Active Astronaut with Multiple Duties
            var person1 = new Person { Name = "John Smith" };
            people.Add(person1);
            
            var duty1_1 = new AstronautDuty
            {
                Person = person1,
                Rank = "Captain",
                DutyTitle = "Mission Specialist",
                DutyStartDate = new DateTime(2015, 3, 15),
                DutyEndDate = new DateTime(2018, 6, 14) // Day before next duty starts
            };
            
            var duty1_2 = new AstronautDuty
            {
                Person = person1,
                Rank = "Captain",
                DutyTitle = "Flight Engineer",
                DutyStartDate = new DateTime(2018, 6, 15),
                DutyEndDate = new DateTime(2022, 1, 31) // Day before current duty starts
            };
            
            var duty1_3 = new AstronautDuty
            {
                Person = person1,
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2022, 2, 1),
                DutyEndDate = null // Current duty
            };
            
            duties.AddRange(new[] { duty1_1, duty1_2, duty1_3 });

            // Sample Astronaut 2: Recently Retired
            var person2 = new Person { Name = "Sarah Johnson" };
            people.Add(person2);
            
            var duty2_1 = new AstronautDuty
            {
                Person = person2,
                Rank = "Colonel",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2012, 5, 10),
                DutyEndDate = new DateTime(2017, 11, 30)
            };
            
            var duty2_2 = new AstronautDuty
            {
                Person = person2,
                Rank = "Colonel",
                DutyTitle = "Mission Specialist",
                DutyStartDate = new DateTime(2017, 12, 1),
                DutyEndDate = new DateTime(2023, 8, 14)
            };
            
            var duty2_3 = new AstronautDuty
            {
                Person = person2,
                Rank = "Colonel",
                DutyTitle = "RETIRED",
                DutyStartDate = new DateTime(2023, 8, 15),
                DutyEndDate = null // Retired duty has no end date
            };
            
            duties.AddRange(new[] { duty2_1, duty2_2, duty2_3 });

            // Sample Astronaut 3: Long Career, Multiple Duties
            var person3 = new Person { Name = "Michael Chen" };
            people.Add(person3);
            
            var duty3_1 = new AstronautDuty
            {
                Person = person3,
                Rank = "Major",
                DutyTitle = "Mission Specialist",
                DutyStartDate = new DateTime(2008, 1, 20),
                DutyEndDate = new DateTime(2011, 3, 19)
            };
            
            var duty3_2 = new AstronautDuty
            {
                Person = person3,
                Rank = "Lieutenant Colonel",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2011, 3, 20),
                DutyEndDate = new DateTime(2015, 9, 29)
            };
            
            var duty3_3 = new AstronautDuty
            {
                Person = person3,
                Rank = "Colonel",
                DutyTitle = "Flight Engineer",
                DutyStartDate = new DateTime(2015, 9, 30),
                DutyEndDate = new DateTime(2020, 4, 14)
            };
            
            var duty3_4 = new AstronautDuty
            {
                Person = person3,
                Rank = "Colonel",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2020, 4, 15),
                DutyEndDate = null // Current duty
            };
            
            duties.AddRange(new[] { duty3_1, duty3_2, duty3_3, duty3_4 });

            // Sample Astronaut 4: New Astronaut with Single Duty
            var person4 = new Person { Name = "Emily Rodriguez" };
            people.Add(person4);
            
            var duty4_1 = new AstronautDuty
            {
                Person = person4,
                Rank = "Captain",
                DutyTitle = "Mission Specialist",
                DutyStartDate = new DateTime(2024, 1, 10),
                DutyEndDate = null // Current duty
            };
            
            duties.Add(duty4_1);

            // Sample Astronaut 5: Person Without Astronaut Assignment
            var person5 = new Person { Name = "David Williams" };
            people.Add(person5);
            // No duties for this person - demonstrates rule: "Person without astronaut assignment has no Astronaut records"

            // Sample Astronaut 6: Early Retirement
            var person6 = new Person { Name = "Lisa Anderson" };
            people.Add(person6);
            
            var duty6_1 = new AstronautDuty
            {
                Person = person6,
                Rank = "Major",
                DutyTitle = "Mission Specialist",
                DutyStartDate = new DateTime(2016, 7, 1),
                DutyEndDate = new DateTime(2021, 5, 31)
            };
            
            var duty6_2 = new AstronautDuty
            {
                Person = person6,
                Rank = "Major",
                DutyTitle = "RETIRED",
                DutyStartDate = new DateTime(2021, 6, 1),
                DutyEndDate = null
            };
            
            duties.AddRange(new[] { duty6_1, duty6_2 });

            // Sample Astronaut 7: Active with Long Single Duty
            var person7 = new Person { Name = "Robert Martinez" };
            people.Add(person7);
            
            var duty7_1 = new AstronautDuty
            {
                Person = person7,
                Rank = "Colonel",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2019, 3, 1),
                DutyEndDate = null // Long-running current duty
            };
            
            duties.Add(duty7_1);

            // Add all people first
            await context.People.AddRangeAsync(people);
            await context.SaveChangesAsync();

            // Add all duties
            await context.AstronautDuties.AddRangeAsync(duties);
            await context.SaveChangesAsync();

            logger.LogInformation("Database seeded successfully with {PersonCount} people and {DutyCount} duties", 
                people.Count, duties.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding database");
            throw;
        }
    }
}

