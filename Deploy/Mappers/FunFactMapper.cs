using Deploy.DTOs;
using Deploy.Models;

namespace Deploy.Mappers;

public static class FunFactMapper
{
    public static AnimalFunFactDto ToAnimalFunFactDto(
        AnimalFunFact fact, ProfileProgress progress, HashSet<int> unlockedFactIds)
    {
        return new AnimalFunFactDto
        {
            Id             = fact.Id,
            Emoji          = fact.Emoji,
            FactText       = fact.FactText,
            FactImageUrl   = fact.FactImageUrl,
            FactOrder      = fact.FactOrder,
            UnlockLevel    = fact.UnlockLevel,
            IsLocked       = fact.IsLocked,
            AccessStatus   = fact.IsLocked || fact.UnlockLevel > progress.CurrentLevel
                                 ? "locked"
                                 : "unlocked",
            LevelsNeeded   = Math.Max(fact.UnlockLevel - progress.CurrentLevel, 0),
            UserLevel      = progress.CurrentLevel,
            UserPoints     = progress.TotalPoints,
            AlreadyUnlocked = unlockedFactIds.Contains(fact.Id)
        };
    }

    public static IEnumerable<AnimalFunFactDto> ToAnimalFunFactDtoList(
        IEnumerable<AnimalFunFact> facts, ProfileProgress progress, HashSet<int> unlockedFactIds)
    {
        return facts.Select(f => ToAnimalFunFactDto(f, progress, unlockedFactIds));
    }
}
