#region

using System.Collections.Generic;
using Game.Battle;
using Game.Data;
using System.Linq;

#endregion

namespace Game.Comm.Channel
{
    public interface IBattleChannel
    {
        void BattleEnterRound(IBattleManager battle, ICombatList atk, ICombatList def, uint round);
        void BattleExitBattle(IBattleManager battle, ICombatList atk, ICombatList def);
        void BattleReinforceDefender(IBattleManager battle, IEnumerable<CombatObject> list);
        void BattleReinforceAttacker(IBattleManager battle, IEnumerable<CombatObject> list);
        void BattleSkippedAttacker(IBattleManager battle, CombatObject source);
        void BattleActionAttacked(IBattleManager battle, CombatObject source, CombatObject target, decimal damage);
        void BattleWithdrawDefender(IBattleManager battle, IEnumerable<CombatObject> list);
        void BattleWithdrawAtacker(IBattleManager battle, IEnumerable<CombatObject> list);
    }

    public class BattleChannel : IBattleChannel
    {
        private readonly string channelName;

        public BattleChannel(ICity city)
        {
            channelName = "/BATTLE/" + city.Id;
        }

        private Packet CreatePacket(IBattleManager battle, Command command)
        {
            var packet = new Packet(command);
            packet.AddUInt32(battle.BattleId);

            return packet;
        }

        public void BattleEnterRound(IBattleManager battle, ICombatList atk, ICombatList def, uint round)
        {
            var packet = CreatePacket(battle, Command.BattleNewRound);            
            packet.AddUInt32(round);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleExitBattle(IBattleManager battle, ICombatList atk, ICombatList def)
        {
            var packet = CreatePacket(battle, Command.BattleEnded);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleReinforceDefender(IBattleManager battle, IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var packet = CreatePacket(battle, Command.BattleReinforceDefender);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleReinforceAttacker(IBattleManager battle, IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var packet = CreatePacket(battle, Command.BattleReinforceAttacker);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleSkippedAttacker(IBattleManager battle, CombatObject source)
        {
            var packet = CreatePacket(battle, Command.BattleSkipped);
            packet.AddUInt32(source.Id);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleActionAttacked(IBattleManager battle, CombatObject source, CombatObject target, decimal damage)
        {
            var packet = CreatePacket(battle, Command.BattleAttack);
            packet.AddUInt32(source.Id);
            packet.AddUInt32(target.Id);
            packet.AddFloat((float)damage);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleWithdrawDefender(IBattleManager battle, IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var combatObject = combatObjectList.First();
            var packet = CreatePacket(battle, Command.BattleWithdrawDefender);
            packet.AddUInt32(combatObject.TroopStub.City.Id);
            packet.AddByte(combatObject.TroopStub.TroopId);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleWithdrawAtacker(IBattleManager battle, IEnumerable<CombatObject> list)
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