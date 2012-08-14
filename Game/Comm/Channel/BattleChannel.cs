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
        void BattleEnterRound(uint battleId, ICombatList atk, ICombatList def, uint round);
        void BattleExitBattle(uint battleId, ICombatList atk, ICombatList def);
        void BattleReinforceDefender(uint battleId, IEnumerable<CombatObject> list);
        void BattleReinforceAttacker(uint battleId, IEnumerable<CombatObject> list);
        void BattleSkippedAttacker(uint battleId, CombatObject source);
        void BattleActionAttacked(uint battleId, CombatObject source, CombatObject target, decimal damage);
        void BattleWithdrawDefender(uint battleId, IEnumerable<CombatObject> list);
        void BattleWithdrawAtacker(uint battleId, IEnumerable<CombatObject> list);
    }

    public class BattleChannel : IBattleChannel
    {
        private readonly string channelName;

        public BattleChannel(ICity city)
        {
            channelName = "/BATTLE/" + city.Id;
        }

        private Packet CreatePacket(uint battleId, Command command)
        {
            var packet = new Packet(command);
            packet.AddUInt32(battleId);

            return packet;
        }

        public void BattleEnterRound(uint battleId, ICombatList atk, ICombatList def, uint round)
        {
            var packet = CreatePacket(battleId, Command.BattleNewRound);            
            packet.AddUInt32(round);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleExitBattle(uint battleId, ICombatList atk, ICombatList def)
        {
            var packet = CreatePacket(battleId, Command.BattleEnded);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleReinforceDefender(uint battleId, IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var packet = CreatePacket(battleId, Command.BattleReinforceDefender);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleReinforceAttacker(uint battleId, IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var packet = CreatePacket(battleId, Command.BattleReinforceAttacker);
            PacketHelper.AddToPacket(combatObjectList, packet);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleSkippedAttacker(uint battleId, CombatObject source)
        {
            var packet = CreatePacket(battleId, Command.BattleSkipped);
            packet.AddUInt32(source.Id);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleActionAttacked(uint battleId, CombatObject source, CombatObject target, decimal damage)
        {
            var packet = CreatePacket(battleId, Command.BattleAttack);
            packet.AddUInt32(source.Id);
            packet.AddUInt32(target.Id);
            packet.AddFloat((float)damage);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleWithdrawDefender(uint battleId, IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var combatObject = combatObjectList.First();
            var packet = CreatePacket(battleId, Command.BattleWithdrawDefender);
            packet.AddUInt32(combatObject.TroopStub.City.Id);
            packet.AddByte(combatObject.TroopStub.TroopId);
            Global.Channel.Post(channelName, packet);
        }

        public void BattleWithdrawAtacker(uint battleId, IEnumerable<CombatObject> list)
        {
            var combatObjectList = new List<CombatObject>(list);
            var combatObject = combatObjectList.First();
            var packet = CreatePacket(battleId, Command.BattleWithdrawAttacker);
            packet.AddUInt32(combatObject.TroopStub.City.Id);
            packet.AddByte(combatObject.TroopStub.TroopId);
            Global.Channel.Post(channelName, packet);
        }
    }
}