using Game.Data;

namespace Game.Logic
{
    public class ActionRequirement
    {
        public uint EffectReqId { get; set; }

        public EffectInheritance EffectReqInherit { get; set; }

        public byte Index { get; set; }

        public ActionOption Option { get; set; }

        public string[] Parms { get; set; }

        public ActionType Type { get; set; }
    }
}