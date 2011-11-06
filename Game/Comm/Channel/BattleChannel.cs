#region

using System.Collections.Generic;
using Game.Battle;
using Game.Data;

#endregion

namespace Game.Comm.Channel
{
    public interface IBattleChannel
    {
        void BattleEnterRound(CombatList atk, CombatList def, uint round);
        void BattleExitBattle(CombatList atk, CombatList def);
        void BattleReinforceDefender(IEnumerable<CombatObject> list);
        void BattleReinforceAttacker(IEnumerable<CombatObject> list);
        void BattleSkippedAttacker(CombatObject source);
        void BattleActionAttacked(CombatObject source, CombatObject target, ushort damage);
    }

    public class BattleChannel : IBattleChannel
    {
        private readonly string channelName;

        public BattleChannel(City city)
        {
            channelName = "/BATTLE/" + city.Id;
        }

        public void BattleEnterRound(CombatList atk, CombatList def, uint round)
        {
            var packet = new Packet(Command.BattleNewRound);
            packet.AddUInt32(round);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleExitBattle(CombatList atk, CombatList def)
        {
            var packet = new Packet(Command.BattleEnded);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleReinforceDefender(IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var packet = new Packet(Command.BattleReinforceDefender);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleReinforceAttacker(IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var packet = new Packet(Command.BattleReinforceAttacker);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleSkippedAttacker(CombatObject source)
        {
            var packet = new Packet(Command.BattleSkipped);
            packet.AddUInt32(source.Id);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleActionAttacked(CombatObject source, CombatObject target, ushort damage)
        {
            var packet = new Packet(Command.BattleAttack);
            packet.AddUInt32(source.Id);
            packet.AddUInt32(target.Id);
            packet.AddUInt16(damage);
            Global.Channel.Post(channelName, packet);
        }
    }
}