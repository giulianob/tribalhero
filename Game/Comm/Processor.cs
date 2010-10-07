#region

using System;
using System.Collections.Generic;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Comm {
    public partial class Processor {
        public delegate void DoWork(Session session, Packet packet);

        private readonly Dictionary<Command, ProcessorCommand> commands = new Dictionary<Command, ProcessorCommand>();
        private readonly Dictionary<Command, ProcessorCommand> events = new Dictionary<Command, ProcessorCommand>();

        private class ProcessorCommand {
            public readonly DoWork function;

            public ProcessorCommand(DoWork function) {
                this.function = function;
            }
        }

        public Processor() {
            RegisterCommand(Command.LOGIN, CmdLogin);
            RegisterCommand(Command.CMD_LINE, CmdLineCommand);
            RegisterCommand(Command.CITY_CREATE_INITIAL, CmdCreateInitialCity);
            RegisterCommand(Command.QUERY_XML, CmdQueryXml);
            RegisterCommand(Command.REGION_GET, CmdGetRegion);
            RegisterCommand(Command.CITY_REGION_GET, CmdGetCityRegion);
            RegisterCommand(Command.STRUCTURE_INFO, CmdGetStructureInfo);
            RegisterCommand(Command.FOREST_INFO, CmdGetForestInfo);
            RegisterCommand(Command.FOREST_CAMP_CREATE, CmdCreateForestCamp);
            RegisterCommand(Command.STRUCTURE_BUILD, CmdCreateStructure);
            RegisterCommand(Command.STRUCTURE_UPGRADE, CmdUpgradeStructure);
            RegisterCommand(Command.STRUCTURE_DOWNGRADE, CmdDowngradeStructure);
            RegisterCommand(Command.STRUCTURE_CHANGE, CmdChangeStructure);
            RegisterCommand(Command.STRUCTURE_LABOR_MOVE, CmdLaborMove);
            RegisterCommand(Command.UNIT_TRAIN, CmdTrainUnit);
            RegisterCommand(Command.UNIT_UPGRADE, CmdUnitUpgrade);
            RegisterCommand(Command.ACTION_CANCEL, CmdCancelAction);
            RegisterCommand(Command.TECH_UPGRADE, CmdTechnologyUpgrade);
            RegisterCommand(Command.TROOP_INFO, CmdGetTroopInfo);
            RegisterCommand(Command.TROOP_ATTACK, CmdTroopAttack);
            RegisterCommand(Command.TROOP_DEFEND, CmdTroopDefend);
            RegisterCommand(Command.TROOP_RETREAT, CmdTroopRetreat);
            RegisterCommand(Command.TROOP_LOCAL_SET, CmdLocalTroopSet);
            RegisterCommand(Command.PLAYER_USERNAME_GET, CmdGetUsername);
            RegisterCommand(Command.CITY_USERNAME_GET, CmdGetCityUsername);
            RegisterCommand(Command.BATTLE_SUBSCRIBE, CmdBattleSubscribe);
            RegisterCommand(Command.BATTLE_UNSUBSCRIBE, CmdBattleUnsubscribe);
            RegisterCommand(Command.MARKET_BUY, CmdMarketBuy);
            RegisterCommand(Command.MARKET_SELL, CmdMarketSell);
            RegisterCommand(Command.MARKET_PRICES, CmdMarketGetPrices);
            RegisterCommand(Command.NOTIFICATION_LOCATE, CmdNotificationLocate);
            RegisterCommand(Command.CITY_LOCATE, CmdCityLocate);
            RegisterCommand(Command.CITY_LOCATE_BY_NAME, CmdCityLocateByName);
            RegisterCommand(Command.REGION_ROAD_BUILD, CmdRoadCreate);
            RegisterCommand(Command.REGION_ROAD_DESTROY, CmdRoadDestroy);
            RegisterCommand(Command.CITY_RESOURCE_SEND, CmdSendResources);

            RegisterEvent(Command.ON_DISCONNECT, EventOnDisconnect);
            RegisterEvent(Command.ON_CONNECT, EventOnConnect);
        }

        protected void RegisterCommand(Command cmd, DoWork func) {
            commands[cmd] = new ProcessorCommand(func);
        }

        protected void RegisterEvent(Command cmd, DoWork func) {
            events[cmd] = new ProcessorCommand(func);
        }

        public void ReplySuccess(Session session, Packet packet) {
            Packet reply = new Packet(packet) {
                                                  Option = (ushort) Packet.Options.REPLY
                                              };
            session.Write(reply);
        }

        public Packet ReplyError(Session session, Packet packet, Error error) {
            return ReplyError(session, packet, error, true);
        }

        public Packet ReplyError(Session session, Packet packet, Error error, bool sendPacket) {
            Packet reply = new Packet(packet) {
                                                  Option = (ushort) Packet.Options.FAILED | (ushort) Packet.Options.REPLY
                                              };
            reply.AddInt32((int) error);

            if (sendPacket)
                session.Write(reply);

            return reply;
        }

        public virtual void Execute(Session session, Packet packet) {
#if DEBUG || CHECK_LOCKS
            Global.Logger.Info(packet.ToString(32));
#endif

            lock (session) {
                ProcessorCommand command;
                if (!commands.TryGetValue(packet.Cmd, out command))
                    return;

                command.function(session, packet);
            }
        }

        public virtual void ExecuteEvent(Session session, Packet packet) {
#if DEBUG || CHECK_LOCKS
            Global.Logger.Info("Event: " + packet.ToString(32));
#endif

            lock (session) {
                events[packet.Cmd].function(session, packet);
            }
        }
    }
}