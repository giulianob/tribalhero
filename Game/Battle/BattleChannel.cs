using System;
using System.Collections.Generic;
using Game.Comm;
using Game.Data;

namespace Game.Battle {
    public class BattleChannel {
        private readonly string channelName;
        private readonly BattleManager battle;

        public BattleChannel(BattleManager battle, string channelName) {
            this.battle = battle;
            this.channelName = channelName;
            battle.ActionAttacked += BattleActionAttacked;
            battle.SkippedAttacker += BattleSkippedAttacker;
            battle.ReinforceAttacker += BattleReinforceAttacker;
            battle.ReinforceDefender += BattleReinforceDefender;
            battle.ExitBattle += BattleExitBattle;
            battle.EnterRound += BattleEnterRound;
        }

        private void BattleEnterRound(CombatList atk, CombatList def, uint round) {
            Packet packet = new Packet(Command.BATTLE_NEW_ROUND);
            packet.AddUInt32(round);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleExitBattle(CombatList atk, CombatList def) {
            Packet packet = new Packet(Command.BATTLE_ENDED);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleReinforceDefender(IEnumerable<CombatObject> list) {
            List<CombatObject> combatObjectList = new List<CombatObject>(list);
            Packet packet = new Packet(Command.BATTLE_REINFORCE_DEFENDER);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleReinforceAttacker(IEnumerable<CombatObject> list) {
            List<CombatObject> combatObjectList = new List<CombatObject>(list);
            Packet packet = new Packet(Command.BATTLE_REINFORCE_ATTACKER);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleSkippedAttacker(CombatObject source) {
            Packet packet = new Packet(Command.BATTLE_SKIPPED);
            packet.AddUInt32(source.Id);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleActionAttacked(CombatObject source, CombatObject target, ushort damage) {
            Packet packet = new Packet(Command.BATTLE_ATTACK);
            packet.AddUInt32(source.Id);
            packet.AddUInt32(target.Id);
            packet.AddUInt16(damage);
            Global.Channel.Post(channelName, packet);
        }
    }
}