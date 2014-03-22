<?php

class ActionsController extends AppController {
    var $uses = array('Message', 'Player', 'Report', 'MessageBoardThread');

    var $allowedFromGame = array('unread');
   
    function unread() {
        if (!array_key_exists('playerId', $this->request->data)) return;

        $playerId = $this->request->data['playerId'];
        
        $unreadMessages = $this->Message->getUnreadCount($playerId);
        $unreadReports = $this->Report->getUnreadCount($playerId);
        
        $unreadMessageBoard = 0;
        $tribeId = $this->Player->getTribeId($playerId);
        if ($tribeId) {
            $unreadMessageBoard = $this->MessageBoardThread->getUnreadMessage($playerId, $tribeId);
        }
        
        $data = array('unreadMessages' => $unreadMessages, 'unreadReports' => $unreadReports, 'unreadForum' => $unreadMessageBoard);

        $this->set(compact('data'));
        $this->render('/elements/to_json');
    }
}
