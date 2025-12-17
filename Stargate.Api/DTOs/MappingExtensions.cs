using StargateAPI.Domain.Models;
using StargateAPI.DTOs;

namespace StargateAPI.DTOs;

public static class MappingExtensions
{
    public static PersonDto ToDto(this Person person)
    {
        return new PersonDto
        {
            PersonId = person.Id,
            Name = person.Name,
            CurrentRank = person.AstronautDetail?.CurrentRank,
            CurrentDutyTitle = person.AstronautDetail?.CurrentDutyTitle,
            CareerStartDate = person.AstronautDetail?.CareerStartDate,
            CareerEndDate = person.AstronautDetail?.CareerEndDate
        };
    }

    public static AstronautDutyDto ToDto(this AstronautDuty duty)
    {
        return new AstronautDutyDto
        {
            Id = duty.Id,
            PersonId = duty.PersonId,
            Rank = duty.Rank,
            DutyTitle = duty.DutyTitle,
            DutyStartDate = duty.DutyStartDate,
            DutyEndDate = duty.DutyEndDate
        };
    }

    public static List<PersonDto> ToDto(this IEnumerable<Person> people)
    {
        return people.Select(p => p.ToDto()).ToList();
    }

    public static List<AstronautDutyDto> ToDto(this IEnumerable<AstronautDuty> duties)
    {
        return duties.Select(d => d.ToDto()).ToList();
    }
}

