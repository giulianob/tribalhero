package src.Comm.Commands
{
	import flash.events.Event;
	import src.Comm.*;
	import src.Global;
	import src.Map.MapComm;
	import src.UI.Dialog.TribeProfileDialog;

	public class MessagingComm
	{
		private var mapComm: MapComm;
		private var session: Session;
		private var unreadLoader: GameURLLoader;

		public function MessagingComm(mapComm: MapComm) {
			this.mapComm = mapComm;
			this.session = mapComm.session;
			
			unreadLoader = new GameURLLoader();
			unreadLoader.addEventListener(Event.COMPLETE, onReceiveUnread);
			
			session.addEventListener(Commands.CHANNEL_NOTIFICATION, onChannelReceive);
		}		
		
		public function dispose() : void {			
			session.removeEventListener(Commands.CHANNEL_NOTIFICATION, onChannelReceive);
		}
		
		public function onChannelReceive(e: PacketEvent):void
		{
			switch(e.packet.cmd)
			{				
				case Commands.BATTLE_REPORT_UNREAD:
					onReportUnreadUpdate(e.packet);
				break;
				case Commands.MESSAGE_UNREAD:
					onMessageUnreadUpdate(e.packet);
				break;
				case Commands.FORUM_UNREAD:
					onForumUnreadUpdate(e.packet);
				break;
				case Commands.REFRESH_UNREAD:
					onRefreshUnread(e.packet);
				break;
			}
		}				
				
		/**
		 * Forces client to refresh unread.
		 * This is only used right now for system notifications since server doesnt know 
		 * how many messages the client should display so it just tells it to refresh.
		 * @param	packet
		 */
		private function onRefreshUnread(packet: Packet): void 
		{
			refreshUnreadCounts();
		}
		
		private function onReportUnreadUpdate(packet: Packet): void
		{
			Global.gameContainer.setUnreadBattleReportCount(packet.readInt());
		}
		
		private function onMessageUnreadUpdate(packet: Packet): void
		{
			Global.gameContainer.setUnreadMessageCount(packet.readInt());
		}
		
		private function onForumUnreadUpdate(packet: Packet): void
		{
			Global.gameContainer.setUnreadForumIcon(true);
			var tribeProfileDialog: TribeProfileDialog = Global.gameContainer.findDialog(TribeProfileDialog); 
			if (tribeProfileDialog) {
				tribeProfileDialog.ReceiveNewMessage();
			}
		}		
		
		public function refreshUnreadCounts(): void {
			try {
				unreadLoader.load("/actions/unread", [ ], true, false);
			}
			catch (e: Error) { }
		}
		
		private function onReceiveUnread(e: Event = null) : void {
			try {
				var data: Object = unreadLoader.getDataAsObject();
				
				var unreadMessages: int = data.unreadMessages;			
				var unreadReports: int = data.unreadReports;
				var unreadForum: int = data.unreadForum;
				
				Global.gameContainer.setUnreadBattleReportCount(unreadReports);
				Global.gameContainer.setUnreadMessageCount(unreadMessages);
				if (unreadForum) {
					Global.gameContainer.setUnreadForumIcon(true);
				}
			}
			catch (e: Error) { }
		}		

		public function list(loader: GameURLLoader, folder: String, page: int) : void {
			loader.load("/messages/listing", [ { key: "folder", value: folder }, { key: "page", value: page } ]);
		}
		
		public function del(loader: GameURLLoader, ids: Array) : void {
			loader.load("/messages/del", [ { key: "ids", value: ids } ]);
		}
		
		public function markAsRead(loader: GameURLLoader, ids: Array) : void {
			loader.load("/messages/mark_as_read", [ { key: "ids", value: ids } ]);
		}		
		
		public function view(loader: GameURLLoader, id: int) : void {
			loader.load("/messages/view", [ { key: "id", value: id } ]);
		}		
		
		public function send(loader: GameURLLoader, to: String, subject: String, message: String) : void {
			loader.load("/messages/send", [ { key: "to", value: to }, { key: "subject", value: subject }, { key: "message", value: message } ]);
		}		
	}

}

