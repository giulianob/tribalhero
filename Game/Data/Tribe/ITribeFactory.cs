using System;

namespace Game.Data.Tribe
{
    public interface ITribeFactory
    {
        ITribe CreateTribe(IPlayer owner,
                           string name,
                           string desc,
                           byte level,
                           decimal victoryPoints,
                           int attackPoints,
                           int defensePoints,
                           Resource resource,
                           DateTime created);

        ITribe CreateTribe(IPlayer owner, string name);
    }
}