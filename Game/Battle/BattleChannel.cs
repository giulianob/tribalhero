#region

using System.Collections.Generic;
using Game.Comm;
using Game.Data;

#endregion

namespace Game.Battle
{
    public class BattleChannel
    {
        private readonly string channelName;

        public BattleChannel(BattleManager battle, string channelName)
        {
            this.channelName = channelName;
            battle.ActionAttacked += BattleActionAttacked;
            battle.SkippedAttacker += BattleSkippedAttacker;
            battle.ReinforceAttacker += BattleReinforceAttacker;
            battle.ReinforceDefender += BattleReinforceDefender;
            battle.ExitBattle += BattleExitBattle;
            battle.EnterRound += BattleEnterRound;
        }

        private void BattleEnterRound(CombatList atk, CombatList def, uint round)
        {
            var packet = new Packet(Command.BattleNewRound);
            packet.AddUInt32(round);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleExitBattle(CombatList atk, CombatList def)
        {
            var packet = new Packet(Command.BattleEnded);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleReinforceDefender(IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var packet = new Packet(Command.BattleReinforceDefender);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleReinforceAttacker(IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var packet = new Packet(Command.BattleReinforceAttacker);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleSkippedAttacker(CombatObject source)
        {
            var packet = new Packet(Command.BattleSkipped);
            packet.AddUInt32(source.Id);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleActionAttacked(CombatObject source, CombatObject target, ushort damage)
        {
            var packet = new Packet(Command.BattleAttack);
            packet.AddUInt32(source.Id);
            packet.AddUInt32(target.Id);
            packet.AddUInt16(damage);
            Global.Channel.Post(channelName, packet);
        }
    }
}