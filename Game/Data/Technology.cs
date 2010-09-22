#region

using System.Collections.Generic;

#endregion

namespace Game.Data {
    public class TechnologyBase {
        public string name;
        public uint techtype;
        public byte level;
        public uint time;
        public Resource resources;
        public List<Effect> effects;        

        public int TechnologyHash
        {
            get { return (int)(techtype * 100 + level); }
        }
    }

    public class Technology {        
        public EffectLocation ownerLocation;
        public uint ownerId;

        public Technology(TechnologyBase techBase) {
            TechBase = techBase;
        }

        public TechnologyBase TechBase { get; private set; }

        public uint Type {
            get { return TechBase.techtype; }
            set { TechBase.techtype = value; }
        }

        public byte Level {
            get { return TechBase.level; }
            set { TechBase.level = value; }
        }

        public List<Effect> Effects {
            get { return TechBase.effects; }
        }

        public static bool CheckLocation(EffectLocation technologyLocation, EffectLocation effectPath,
                                         EffectLocation targetLocation) {
            return effectPath == targetLocation;
        }

        internal List<Effect> GetEffects(EffectCode effectCode, EffectInheritance inherit,
                                         EffectLocation targetLocation) {
            List<Effect> list = new List<Effect>();
            bool isSelf = (inherit & EffectInheritance.SELF) == EffectInheritance.SELF;
            bool isInvisible = (inherit & EffectInheritance.INVISIBLE) == EffectInheritance.INVISIBLE;

            foreach (Effect effect in TechBase.effects) {
                if (effect.id != effectCode)
                    continue;
                if (!CheckLocation(ownerLocation, effect.location, targetLocation))
                    continue;

                if (isSelf) {
                    if (!effect.isPrivate)
                        list.Add(effect);
                }

                if (!isInvisible)
                    continue;

                if (effect.isPrivate)
                    list.Add(effect);
            }

            return list;
        }

        public List<Effect> GetAllEffects(EffectInheritance inherit, EffectLocation targetLocation) {
            List<Effect> list = new List<Effect>();
            bool isSelf = (inherit & EffectInheritance.SELF) == EffectInheritance.SELF;
            bool isInvisible = (inherit & EffectInheritance.INVISIBLE) == EffectInheritance.INVISIBLE;

            foreach (Effect effect in TechBase.effects) {
                if (!CheckLocation(ownerLocation, effect.location, targetLocation))
                    continue;

                if (isSelf) {
                    if (!effect.isPrivate)
                        list.Add(effect);
                }

                if (!isInvisible)
                    continue;

                if (effect.isPrivate)
                    list.Add(effect);
            }
            return list;
        }

        public void Print() {
            Global.Logger.Info(string.Format("Technology type[{0}] lvl[{1}] Location[{2}]", TechBase.techtype,
                                             TechBase.level, ownerLocation));
            foreach (Effect effect in TechBase.effects)
                effect.Print();
        }
    }
}