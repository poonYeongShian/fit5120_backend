using Deploy.DTOs;
using Deploy.Models;

namespace Deploy.Mappers;

public static class AnimalMapper
{
    public static AnimalCardDto ToAnimalCardDto(Animal animal, Category? category, ConservationStatus? conservationStatus)
    {
        return new AnimalCardDto
        {
            CommonName = animal.CommonName,
            ScientificName = animal.ScientificName,
            Category = category?.Name ?? string.Empty,
            StatusCode = conservationStatus?.Code ?? string.Empty,
            StatusLabel = conservationStatus?.Label ?? string.Empty,
            ImageUrl = animal.ImageUrl
        };
    }

    public static IEnumerable<AnimalCardDto> ToAnimalCardDtoList(
        IEnumerable<(Animal Animal, Category? Category, ConservationStatus? ConservationStatus)> items)
    {
        return items.Select(i => ToAnimalCardDto(i.Animal, i.Category, i.ConservationStatus));
    }

    public static AnimalCardDetailDto ToAnimalCardDetailDto(Animal animal, Category? category, ConservationStatus? conservationStatus)
    {
        return new AnimalCardDetailDto
        {
            CommonName = animal.CommonName,
            ScientificName = animal.ScientificName,
            CategoryName = category?.Name ?? string.Empty,
            ConservationCode = conservationStatus?.Code ?? string.Empty,
            ConservationLabel = conservationStatus?.Label ?? string.Empty,
            ConservationDescription = conservationStatus?.Description ?? string.Empty,
            ConservationReason = animal.ConservationReason,
            ImageUrl = animal.ImageUrl,
            SeverityOrder = conservationStatus?.SeverityOrder ?? 0,
            Habitat = animal.Habitat,
            Diet = animal.Diet,
            Lifespan = animal.Lifespan,
            Description = animal.Description
        };
    }

    public static AnimalOccurrenceDto ToAnimalOccurrenceDto(AnimalOccurrence occurrence)
    {
        return new AnimalOccurrenceDto
        {
            LocationName = occurrence.LocationName,
            Latitude = occurrence.Latitude,
            Longitude = occurrence.Longitude,
            ObservedAt = occurrence.ObservedAt,
            Notes = occurrence.Notes
        };
    }

    public static IEnumerable<AnimalOccurrenceDto> ToAnimalOccurrenceDtoList(IEnumerable<AnimalOccurrence> occurrences)
    {
        return occurrences.Select(ToAnimalOccurrenceDto);
    }
}
