#region

using System;
using Game.Util;

#endregion

namespace Game.Data
{
    public class Effect
    {
        public object[] Value = new object[5];

        public EffectCode Id { get; set; }

        public bool IsPrivate { get; set; }

        public EffectLocation Location { get; set; }

        public void Print()
        {
            LoggerFactory.Current.GetCurrentClassLogger().Info(string.Format("Effect[{0} isPrivate[{1}] Location[{2}]", Id, IsPrivate, Location));
        }
    }

    public enum EffectLocation
    {
        Object = 0,

        City = 1,

        Player = 2
    }

    [Flags]
    public enum EffectInheritance
    {
        Invisible = 1,

        Self = 2,

        SelfAll = 3,

        Upward = 4,

        All = 7
    }

    public enum EffectCode
    {
        BuildTimeMultiplier = 0,

        ResourceRate = 1,

        BuildHeavyTank = 2,

        TrainTimeMultiplier = 3,

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

        AtticStorageMod = 16,

        XFor1 = 17,

        TradeSpeedMod = 18,

        SunDance = 19,

        LaborMoveTimeMod = 20,

        SenseOfUrgency = 21,

        LaborTrainTimeMod = 22,

        LootLoadMod = 23,

        WeaponExport = 24,

        LastStand = 25,

        SplashReduction = 26,

        BattleStatsArmoryMod = 101,

        BattleStatsBlacksmithMod = 102,

        UpkeepReduce = 103,

        UnitTrainTimeFirst15Reduction = 104
    }
}