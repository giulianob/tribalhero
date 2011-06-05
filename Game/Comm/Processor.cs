#region

using System.Collections.Generic;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Comm
{
    public partial class Processor
    {
        #region Delegates

        public delegate void DoWork(Session session, Packet packet);

        #endregion

        private readonly Dictionary<Command, ProcessorCommand> commands = new Dictionary<Command, ProcessorCommand>();
        private readonly Dictionary<Command, ProcessorCommand> events = new Dictionary<Command, ProcessorCommand>();

        public Processor()
        {
            RegisterCommand(Command.Login, CmdLogin);
            RegisterCommand(Command.PlayerProfile, CmdGetProfile);
            RegisterCommand(Command.PlayerDescriptionSet, CmdSetPlayerDescription);
            RegisterCommand(Command.CmdLine, CmdLineCommand);
            RegisterCommand(Command.CityCreateInitial, CmdCreateInitialCity);
            RegisterCommand(Command.QueryXml, CmdQueryXml);
            RegisterCommand(Command.RegionGet, CmdGetRegion);
            RegisterCommand(Command.CityRegionGet, CmdGetCityRegion);
            RegisterCommand(Command.StructureInfo, CmdGetStructureInfo);
            RegisterCommand(Command.ForestInfo, CmdGetForestInfo);
            RegisterCommand(Command.ForestCampCreate, CmdCreateForestCamp);

            RegisterCommand(Command.StructureBuild, CmdCreateStructure);
            RegisterCommand(Command.StructureUpgrade, CmdUpgradeStructure);
            RegisterCommand(Command.StructureDowngrade, CmdDowngradeStructure);
            RegisterCommand(Command.StructureChange, CmdChangeStructure);
            RegisterCommand(Command.StructureLaborMove, CmdLaborMove);
            RegisterCommand(Command.StructureSelfDestroy, CmdSelfDestroyStructure);

            RegisterCommand(Command.UnitTrain, CmdTrainUnit);
            RegisterCommand(Command.UnitUpgrade, CmdUnitUpgrade);
            RegisterCommand(Command.ActionCancel, CmdCancelAction);
            RegisterCommand(Command.TechUpgrade, CmdTechnologyUpgrade);
            RegisterCommand(Command.TroopInfo, CmdGetTroopInfo);
            RegisterCommand(Command.TroopAttack, CmdTroopAttack);
            RegisterCommand(Command.TroopDefend, CmdTroopDefend);
            RegisterCommand(Command.TroopRetreat, CmdTroopRetreat);
            RegisterCommand(Command.TroopLocalSet, CmdLocalTroopSet);

            RegisterCommand(Command.PlayerUsernameGet, CmdGetUsername);
            RegisterCommand(Command.PlayerNameFromCityName, CmdGetCityOwnerName);
            RegisterCommand(Command.CityUsernameGet, CmdGetCityUsername);

            RegisterCommand(Command.BattleSubscribe, CmdBattleSubscribe);
            RegisterCommand(Command.BattleUnsubscribe, CmdBattleUnsubscribe);

            RegisterCommand(Command.MarketBuy, CmdMarketBuy);
            RegisterCommand(Command.MarketSell, CmdMarketSell);
            RegisterCommand(Command.MarketPrices, CmdMarketGetPrices);

            RegisterCommand(Command.NotificationLocate, CmdNotificationLocate);
            RegisterCommand(Command.CityLocate, CmdCityLocate);
            RegisterCommand(Command.CityLocateByName, CmdCityLocateByName);
            RegisterCommand(Command.RegionRoadBuild, CmdRoadCreate);
            RegisterCommand(Command.RegionRoadDestroy, CmdRoadDestroy);
            RegisterCommand(Command.CityResourceSend, CmdSendResources);
            RegisterCommand(Command.ResourceGather, CmdResourceGather);
            RegisterCommand(Command.CityCreate, CmdCityCreate);

            RegisterCommand(Command.TribeNameGet, CmdTribeName);
            RegisterCommand(Command.TribeInfo, CmdTribeInfo);
            RegisterCommand(Command.TribeCreate, CmdTribeCreate);
            RegisterCommand(Command.TribeDelete, CmdTribeDelete);
            RegisterCommand(Command.TribeUpdate, CmdTribeUpdate);
            RegisterCommand(Command.TribeUpgrade, CmdTribeUpgrade);
            RegisterCommand(Command.TribesmanAdd, CmdTribesmanAdd);
            RegisterCommand(Command.TribesmanRemove, CmdTribesmanRemove);
            RegisterCommand(Command.TribesmanUpdate, CmdTribesmanUpdate);
            RegisterCommand(Command.TribesmanRequest, CmdTribesmanRequest);
            RegisterCommand(Command.TribesmanConfirm, CmdTribesmanConfirm);
            RegisterCommand(Command.TribeAssignementList, CmdTribeAssignmentList);
            RegisterCommand(Command.TribeAssignementCreate, CmdTribeAssignmentCreate);
            RegisterCommand(Command.TribeAssignementJoin, CmdTribeAssignmentJoin);
            RegisterCommand(Command.TribeIncomingList, CmdTribeIncomingList);


            RegisterEvent(Command.OnDisconnect, EventOnDisconnect);
            RegisterEvent(Command.OnConnect, EventOnConnect);
        }

        protected void RegisterCommand(Command cmd, DoWork func)
        {
            commands[cmd] = new ProcessorCommand(func);
        }

        protected void RegisterEvent(Command cmd, DoWork func)
        {
            events[cmd] = new ProcessorCommand(func);
        }

        public void ReplySuccess(Session session, Packet packet)
        {
            var reply = new Packet(packet) {Option = (ushort)Packet.Options.Reply};
            session.Write(reply);
        }

        public Packet ReplyError(Session session, Packet packet, Error error)
        {
            return ReplyError(session, packet, error, true);
        }

        public Packet ReplyError(Session session, Packet packet, Error error, bool sendPacket)
        {
            var reply = new Packet(packet) {Option = (ushort)Packet.Options.Failed | (ushort)Packet.Options.Reply};
            reply.AddInt32((int)error);

            if (sendPacket)
                session.Write(reply);

            return reply;
        }

        public virtual void Execute(Session session, Packet packet)
        {
#if DEBUG || CHECK_LOCKS
            Global.Logger.Info(packet.ToString(32));
#endif

            lock (session)
            {
                ProcessorCommand command;
                if (!commands.TryGetValue(packet.Cmd, out command))
                    return;

                command.Function(session, packet);
            }
        }

        public virtual void ExecuteEvent(Session session, Packet packet)
        {
#if DEBUG || CHECK_LOCKS
            Global.Logger.Info("Event: " + packet.ToString(32));
#endif

            lock (session)
            {
                events[packet.Cmd].Function(session, packet);
            }
        }

        #region Nested type: ProcessorCommand

        private class ProcessorCommand
        {
            public readonly DoWork Function;

            public ProcessorCommand(DoWork function)
            {
                Function = function;
            }
        }

        #endregion
    }
}