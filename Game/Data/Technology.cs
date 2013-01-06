#region

using System.Collections.Generic;

#endregion

namespace Game.Data
{
    public class TechnologyBase
    {
        public List<Effect> Effects { get; set; }

        public byte Level { get; set; }

        public string Name { get; set; }

        public Resource Resources { get; set; }

        public uint Techtype { get; set; }

        public uint Time { get; set; }

        public int TechnologyHash
        {
            get
            {
                return (int)(Techtype * 100 + Level);
            }
        }
    }

    public class Technology
    {
        public Technology(TechnologyBase techBase)
        {
            TechBase = techBase;
        }

        public uint OwnerId { get; set; }

        public EffectLocation OwnerLocation { get; set; }

        public TechnologyBase TechBase { get; private set; }

        public uint Type
        {
            get
            {
                return TechBase.Techtype;
            }
            set
            {
                TechBase.Techtype = value;
            }
        }

        public byte Level
        {
            get
            {
                return TechBase.Level;
            }
            set
            {
                TechBase.Level = value;
            }
        }

        public List<Effect> Effects
        {
            get
            {
                return TechBase.Effects;
            }
        }

        public static bool CheckLocation(EffectLocation technologyLocation,
                                         EffectLocation effectPath,
                                         EffectLocation targetLocation)
        {
            return effectPath == targetLocation;
        }

        internal List<Effect> GetEffects(EffectCode effectCode, EffectInheritance inherit, EffectLocation targetLocation)
        {
            var list = new List<Effect>();
            bool isSelf = (inherit & EffectInheritance.Self) == EffectInheritance.Self;
            bool isInvisible = (inherit & EffectInheritance.Invisible) == EffectInheritance.Invisible;

            foreach (var effect in TechBase.Effects)
            {
                if (effect.Id != effectCode)
                {
                    continue;
                }
                if (!CheckLocation(OwnerLocation, effect.Location, targetLocation))
                {
                    continue;
                }

                if (isSelf)
                {
                    if (!effect.IsPrivate)
                    {
                        list.Add(effect);
                    }
                }

                if (!isInvisible)
                {
                    continue;
                }

                if (effect.IsPrivate)
                {
                    list.Add(effect);
                }
            }

            return list;
        }

        public List<Effect> GetAllEffects(EffectInheritance inherit, EffectLocation targetLocation)
        {
            var list = new List<Effect>();
            bool isSelf = (inherit & EffectInheritance.Self) == EffectInheritance.Self;
            bool isInvisible = (inherit & EffectInheritance.Invisible) == EffectInheritance.Invisible;

            foreach (var effect in TechBase.Effects)
            {
                if (!CheckLocation(OwnerLocation, effect.Location, targetLocation))
                {
                    continue;
                }

                if (isSelf)
                {
                    if (!effect.IsPrivate)
                    {
                        list.Add(effect);
                    }
                }

                if (!isInvisible)
                {
                    continue;
                }

                if (effect.IsPrivate)
                {
                    list.Add(effect);
                }
            }
            return list;
        }

        public void Print()
        {
            Global.Logger.Info(string.Format("Technology type[{0}] lvl[{1}] Location[{2}]",
                                             TechBase.Techtype,
                                             TechBase.Level,
                                             OwnerLocation));
            foreach (var effect in TechBase.Effects)
            {
                effect.Print();
            }
        }
    }
}