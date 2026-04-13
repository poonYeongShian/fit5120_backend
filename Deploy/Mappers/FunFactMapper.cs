using Deploy.DTOs;
using Deploy.Models;

namespace Deploy.Mappers;

public static class FunFactMapper
{
    public static AnimalFunFactDto ToAnimalFunFactDto(
        AnimalFunFact fact, ProfileProgress progress, HashSet<int> unlockedFactIds)
    {
        bool isUnlocked = fact.UnlockLevelNumber <= progress.CurrentLevel
                          || unlockedFactIds.Contains(fact.Id);

        return new AnimalFunFactDto
        {
            Id               = fact.Id,
            Emoji            = fact.Emoji,
            FactText         = fact.FactText,
            FactImageUrl     = fact.FactImageUrl,
            FactOrder        = fact.FactOrder,
            UnlockLevelNumber = fact.UnlockLevelNumber,
            AccessStatus     = isUnlocked ? "unlocked" : "locked",
            LevelsNeeded     = Math.Max(fact.UnlockLevelNumber - progress.CurrentLevel, 0),
            UserLevel        = progress.CurrentLevel,
            UserPoints       = progress.TotalPoints,
            AlreadyUnlocked  = isUnlocked
        };
    }

    public static IEnumerable<AnimalFunFactDto> ToAnimalFunFactDtoList(
        IEnumerable<AnimalFunFact> facts, ProfileProgress progress, HashSet<int> unlockedFactIds)
    {
        return facts.Select(f => ToAnimalFunFactDto(f, progress, unlockedFactIds));
    }
}
