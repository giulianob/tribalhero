struct PlayerUnreadCount {
	1:i32 id,
	2:i32 unreadCount,
}

service Notification {
  void NewMessage(1:PlayerUnreadCount playerUnreadCount);
  void NewTribeForumPost(1:i32 tribeId, 2:i32 unreadCount);
  void NewBattleReport(1:list<PlayerUnreadCount> playerUnreadCount);
}