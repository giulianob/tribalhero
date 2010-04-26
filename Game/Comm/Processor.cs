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
            RegisterCommand(Command.CITY_CREATE_INITIAL, CmdCreateInitialCity);
            RegisterCommand(Command.QUERY_XML, CmdQueryXml);
            RegisterCommand(Command.REGION_GET, CmdGetRegion);
            RegisterCommand(Command.CITY_REGION_GET, CmdGetCityRegion);
            RegisterCommand(Command.STRUCTURE_INFO, CmdGetStructureInfo);
            RegisterCommand(Command.FOREST_INFO, CmdGetForestInfo);
            RegisterCommand(Command.FOREST_CREATE_CAMP, CmdCreateForestCamp);
            RegisterCommand(Command.STRUCTURE_BUILD, CmdCreateStructure);
            RegisterCommand(Command.STRUCTURE_UPGRADE, CmdUpgradeStructure);
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

            RegisterEvent(Command.ON_DISCONNECT, EventOnDisconnect);
            RegisterEvent(Command.ON_CONNECT, EventOnConnect);
        }

        protected bool RegisterCommand(Command cmd, DoWork func) {
            try {
                commands[cmd] = new ProcessorCommand(func);
            }
            catch (Exception) {
                return false;
            }

            return true;
        }

        protected bool RegisterEvent(Command cmd, DoWork func) {
            try {
                events[cmd] = new ProcessorCommand(func);
            }
            catch (Exception) {
                return false;
            }

            return true;
        }

        public void ReplySuccess(Session session, Packet packet) {
            Packet reply = new Packet(packet) {
                                                  Option = (ushort) Packet.Options.REPLY
                                              };
            session.Write(reply);
        }

        public void ReplyError(Session session, Packet packet, Error error) {
            Packet reply = new Packet(packet) {
                                                  Option = (ushort) Packet.Options.FAILED | (ushort) Packet.Options.REPLY
                                              };
            reply.AddInt32((int) error);
            session.Write(reply);
        }

        public virtual void Execute(Session session, Packet packet) {
            Global.Logger.Info(packet.ToString(32));

            lock (session) {
                ProcessorCommand command;
                if (!commands.TryGetValue(packet.Cmd, out command))
                    return;

                command.function(session, packet);
            }
        }

        public virtual void ExecuteEvent(Session session, Packet packet) {
            Global.Logger.Info("Event: " + packet.ToString(32));
            lock (session) {
                events[packet.Cmd].function(session, packet);
            }
        }
    }
}