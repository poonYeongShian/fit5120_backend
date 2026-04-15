using Deploy.DTOs;
using Deploy.Models;

namespace Deploy.Mappers;

public static class AnimalMapper
{
    public static AnimalCardDto ToAnimalCardDto(Animal animal, AnimalGroup? animalGroup, ConservationStatus? conservationStatus)
    {
        return new AnimalCardDto
        {
            Id = animal.Id,
            CommonName = animal.CommonName,
            ScientificName = animal.ScientificName,
            AnimalClass = animalGroup?.GroupName ?? string.Empty,
            StatusCode = conservationStatus?.Code ?? string.Empty,
            StatusLabel = conservationStatus?.Label ?? string.Empty,
            ImageUrl = animal.ImageUrl,
            AvatarPath = animal.AvatarPath
        };
    }

    public static IEnumerable<AnimalCardDto> ToAnimalCardDtoList(
        IEnumerable<(Animal Animal, AnimalGroup? AnimalGroup, ConservationStatus? ConservationStatus)> items)
    {
        return items.Select(i => ToAnimalCardDto(i.Animal, i.AnimalGroup, i.ConservationStatus));
    }

    public static AnimalCardDetailDto ToAnimalCardDetailDto(
        Animal animal,
        AnimalGroup? animalGroup,
        ConservationStatus? conservationStatus,
        IEnumerable<(ThreatDetail Detail, ThreatCategory Category)>? threats = null,
        IEnumerable<(HabitatDetail Detail, HabitatCategory Category)>? habitats = null)
    {
        return new AnimalCardDetailDto
        {
            CommonName = animal.CommonName,
            ScientificName = animal.ScientificName,
            ClassName = animalGroup?.GroupName ?? string.Empty,
            ConservationCode = conservationStatus?.Code ?? string.Empty,
            ConservationLabel = conservationStatus?.Label ?? string.Empty,
            ConservationDescription = conservationStatus?.Description ?? string.Empty,
            ImageUrl = animal.ImageUrl,
            AvatarPath = animal.AvatarPath,
            SeverityOrder = conservationStatus?.SeverityOrder ?? 0,
            Diet = animal.Diet,
            Lifespan = animal.Lifespan,
            Description = animal.Description,
            Threats = threats?.Select(t => new ThreatDetailDto
            {
                ThreatName = t.Category.ThreatName,
                Explanation = t.Detail.Explanation,
                Priority = t.Detail.Priority
            }) ?? [],
            Habitats = habitats?.Select(h => new HabitatDetailDto
            {
                HabitatName = h.Category.HabitatName,
                Priority = h.Detail.Priority,
                Emoji = h.Detail.Emoji
            }) ?? []
        };
    }

    public static AnimalOccurrenceDto ToAnimalOccurrenceDto(AnimalOccurrence occurrence)
    {
        return new AnimalOccurrenceDto
        {
            Latitude = occurrence.Latitude,
            Longitude = occurrence.Longitude
        };
    }

    public static IEnumerable<AnimalOccurrenceDto> ToAnimalOccurrenceDtoList(IEnumerable<AnimalOccurrence> occurrences)
    {
        return occurrences.Select(ToAnimalOccurrenceDto);
    }
}
