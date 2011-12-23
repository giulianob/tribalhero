<?php

class ActionsController extends AppController {
    var $uses = array();

    var $allowedFromGame = array('unread');
   
    function unread() {
        if (!array_key_exists('playerId', $this->params['form'])) return;

        $playerId = $this->params['form']['playerId'];

        $this->Message =& ClassRegistry::init('Message');
        $this->Report =& ClassRegistry::init('Report');

        $unreadMessages = $this->Message->getUnreadCount($playerId);
        $unreadReports = $this->Report->getUnreadCount($playerId);

        $data = array('unreadMessages' => $unreadMessages, 'unreadReports' => $unreadReports);

        $this->set(compact('data'));
        $this->render('/elements/to_json');
    }
}
