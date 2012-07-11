#region

using System.Collections.Generic;
using Game.Battle;
using Game.Battle.CombatObjects;
using Game.Data;
using System.Linq;

#endregion

namespace Game.Comm.Channel
{
    public class BattleChannel
    {
        private readonly string channelName;

        public BattleChannel(IBattleManager battleManager)
        {
            channelName = "/BATTLE/" + battleManager.BattleId;

            battleManager.ActionAttacked += BattleActionAttacked;
            battleManager.SkippedAttacker += BattleSkippedAttacker;
            battleManager.ReinforceAttacker += BattleReinforceAttacker;
            battleManager.ReinforceDefender += BattleReinforceDefender;
            battleManager.ExitBattle += BattleExitBattle;
            battleManager.EnterRound += BattleEnterRound;
            battleManager.WithdrawAttacker += BattleWithdrawAtacker;
            battleManager.WithdrawDefender += BattleWithdrawDefender;
        }

        private Packet CreatePacket(IBattleManager battle, Command command)
        {
            var packet = new Packet(command);
            packet.AddUInt32(battle.BattleId);

            return packet;
        }

        private void BattleEnterRound(IBattleManager battle, ICombatList atk, ICombatList def, uint round)
        {
            var packet = CreatePacket(battle, Command.BattleNewRound);            
            packet.AddUInt32(round);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleExitBattle(IBattleManager battle, ICombatList atk, ICombatList def)
        {
            var packet = CreatePacket(battle, Command.BattleEnded);
            Global.Channel.Post(channelName, packet);

            // Unsubscribe everyone from this channel
            Global.Channel.Unsubscribe("/BATTLE/" + battle.BattleId);
        }

        private void BattleReinforceDefender(IBattleManager battle, IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var packet = CreatePacket(battle, Command.BattleReinforceDefender);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleReinforceAttacker(IBattleManager battle, IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var packet = CreatePacket(battle, Command.BattleReinforceAttacker);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleSkippedAttacker(IBattleManager battle, CombatObject source)
        {
            var packet = CreatePacket(battle, Command.BattleSkipped);
            packet.AddUInt32(source.Id);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleActionAttacked(IBattleManager battle, CombatObject source, CombatObject target, decimal damage)
        {
            var packet = CreatePacket(battle, Command.BattleAttack);
            packet.AddUInt32(source.Id);
            packet.AddUInt32(target.Id);
            packet.AddFloat((float)damage);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleWithdrawDefender(IBattleManager battle, IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var combatObject = combatObjectList.First();
            var packet = CreatePacket(battle, Command.BattleWithdrawDefender);
            packet.AddUInt32(combatObject.TroopStub.City.Id);
            packet.AddByte(combatObject.TroopStub.TroopId);
            Global.Channel.Post(channelName, packet);
        }

        private void BattleWithdrawAtacker(IBattleManager battle, IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var combatObject = combatObjectList.First();
            var packet = CreatePacket(battle, Command.BattleWithdrawAttacker);
            packet.AddUInt32(combatObject.TroopStub.City.Id);
            packet.AddByte(combatObject.TroopStub.TroopId);
            Global.Channel.Post(channelName, packet);
        }
    }
}