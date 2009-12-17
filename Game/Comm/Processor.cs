using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Game.Util;
using Game.Database;
using Game.Setup;

namespace Game.Comm {
    public partial class Processor {
        public delegate void do_work(Session session, Packet packet);
        Dictionary<Command, ProcessorCommand> commands = new Dictionary<Command, ProcessorCommand>();
        Dictionary<Command, ProcessorCommand> events = new Dictionary<Command, ProcessorCommand>();

        class ProcessorCommand {
            public do_work function;
            public ProcessorCommand(do_work function) {
                this.function = function;
            }
        }

        public Processor() {
            registerCommand(Command.LOGIN, CmdLogin);
            registerCommand(Command.QUERY_XML, CmdQueryXml);
            registerCommand(Command.REGION_GET, CmdGetRegion);
            registerCommand(Command.CITY_REGION_GET, CmdGetCityRegion);
            registerCommand(Command.STRUCTURE_INFO, CmdGetStructureInfo);
            registerCommand(Command.STRUCTURE_BUILD, CmdCreateStructure);
            registerCommand(Command.STRUCTURE_UPGRADE, CmdUpgradeStructure);
            registerCommand(Command.STRUCTURE_CHANGE, CmdChangeStructure);
            registerCommand(Command.STRUCTURE_LABOR_MOVE, CmdLaborMove);
            registerCommand(Command.UNIT_TRAIN, CmdTrainUnit);
            registerCommand(Command.UNIT_UPGRADE, CmdUnitUpgrade);
            registerCommand(Command.ACTION_CANCEL, CmdCancelAction);
            registerCommand(Command.TECH_UPGRADE, CmdTechnologyUpgrade);
            registerCommand(Command.TROOP_INFO, CmdGetTroopInfo);
            registerCommand(Command.TROOP_ATTACK, CmdTroopAttack); 
            registerCommand(Command.TROOP_DEFEND, CmdTroopDefend); 
            registerCommand(Command.TROOP_RETREAT, CmdTroopRetreat);
            registerCommand(Command.TROOP_LOCAL_SET, CmdLocalTroopSet);
            registerCommand(Command.PLAYER_USERNAME_GET, CmdGetUsername);
            registerCommand(Command.CITY_USERNAME_GET, CmdGetCityUsername);
            registerCommand(Command.BATTLE_SUBSCRIBE, CmdBattleSubscribe);
            registerCommand(Command.BATTLE_UNSUBSCRIBE, CmdBattleUnsubscribe);
            registerCommand(Command.MARKET_BUY, CmdMarketBuy);
            registerCommand(Command.MARKET_SELL, CmdMarketSell);
            registerCommand(Command.MARKET_PRICES, CmdMarketGetPrices);
            registerCommand(Command.NOTIFICATION_LOCATE, CmdNotificationLocate);

            registerEvent(Command.ON_DISCONNECT, EventOnDisconnect);
            registerEvent(Command.ON_CONNECT, EventOnConnect);
        }

        protected bool registerCommand(Command cmd, do_work func) {
            try {
                commands[cmd] = new ProcessorCommand(func);
            }
            catch (Exception) {
                return false;
            }

            return true;
        }

        protected bool registerEvent(Command cmd, do_work func) {
            try {
                events[cmd] = new ProcessorCommand(func);
            }
            catch (Exception) {
                return false;
            }

            return true;
        }

        public void reply_success(Session session, Packet packet) {
            Packet reply = new Packet(packet);
            reply.Option = (ushort)Packet.Options.REPLY;
            session.write(reply);
        }

        public void reply_error(Session session, Packet packet, Error error) {
            Packet reply = new Packet(packet);
            reply.Option = (ushort)Packet.Options.FAILED | (ushort)Packet.Options.REPLY;
            reply.addInt32((int)error);
            session.write(reply);
        }

        public virtual void execute(Session session, Packet packet) {
            Global.Logger.Info(packet.ToString(256));
            //try {
                ProcessorCommand cmd = commands[packet.Cmd];
                Player player = session.Player;

                lock (session) {
                    cmd.function(session, packet);
                }
            /*}
            catch (Exception e) {
                Global.Logger.Error(string.Format("Session[{0}] Cmd[{1}] failed[{2}]", session.name, Enum.GetName(typeof(Command), packet.Cmd), e));
                Environment.Exit(-1);
            }*/
        }

        public virtual void executeEvent(Session session, Packet packet) {
            Global.Logger.Info("Event: " + packet.ToString(256));
            try {
                lock (session) {
                    events[packet.Cmd].function(session, packet);
                }
            }
            catch (Exception e) {
                Global.Logger.Error(string.Format("Session[{0}] Event[{1}] failed[{2}]", session.name, Enum.GetName(typeof(Command), packet.Cmd), e));
                return;
            }
        }
    }
}
