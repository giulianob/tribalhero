#region

using System;

#endregion

namespace Game.Data {
    public class Effect {
        public EffectCode id;
        public bool isPrivate;
        public object[] value = new object[5];
        public EffectLocation location;

        public void print() {
            Global.Logger.Info(string.Format("Effect[{0} isPrivate[{1}] Location[{2}]", id, isPrivate, location));
        }
    }

    public enum EffectLocation {
        Object = 0,
        City = 1,
        Player = 2,
        Troop = 3
    }

    [Flags]
    public enum EffectInheritance {
        Invisible = 1,
        Self = 2,
        SelfAll = 3,
        Upward = 4,
        All = 7
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

        BattleStatsArmoryMod = 101,
        BattleStatsBlacksmithMod = 102
    }
}