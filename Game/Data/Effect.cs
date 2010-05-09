#region

using System;

#endregion

namespace Game.Data {
    public class Effect {
        public EffectCode id;
        public bool isPrivate;
        public object[] value = new object[5];
        public EffectLocation location;

        public void Print() {
            Global.Logger.Info(string.Format("Effect[{0} isPrivate[{1}] Location[{2}]", id, isPrivate, location));
        }
    }

    public enum EffectLocation {
        OBJECT = 0,
        CITY = 1,
        PLAYER = 2
    }

    [Flags]
    public enum EffectInheritance {
        INVISIBLE = 1,
        SELF = 2,
        SELF_ALL = 3,
        UPWARD = 4,
        ALL = 7
    }

    public enum EffectCode {
        BuildTimeMultiplier = 0,
        ResourceRate = 1,
        BuildHeavyTank = 2,
        TrainTimeMultiplier = 3,
        BuildNinja = 4,
        CanBuild = 5,
        HarvestSurprise = 6,
        OverDigging = 7,
        WoodCutting = 8,
        HaveTechnology = 9,
        CountEffect = 10,
        RepairSaving = 11,
        TroopSpeedMod = 12,
        UnitStatMod = 13,
        AwayFromStructureMod = 14,
        ACallToArmMod = 15,

        BattleStatsArmoryMod = 101,
        BattleStatsBlacksmithMod = 102
    }
}