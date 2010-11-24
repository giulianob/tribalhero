<?php

class MessagesController extends AppController {
    var $helpers = array('Time', 'Text');
    var $allowedFromGame = array('listing', 'view', 'del', 'mark_as_read', 'send');

    function beforeFilter() {
        if (!empty($this->params['named'])) {
            $this->params['form'] = $this->params['named'];
        }
        else {
            Configure::write('debug', 0);
        }

        $this->layout = 'ajax';

        parent::beforeFilter();
    }

    function del() {
        $playerId = $this->params['form']['playerId'];
        $messageIds = array_key_exists('ids', $this->params['form']) ? $this->params['form']['ids'] : array();
        if (!is_array($messageIds)) {
            $messageIds = array($messageIds);
        }

        $this->Message->sendToTrash($playerId, $messageIds);

        $data = array('success' => true);

        $this->set('data', $data);
        $this->render('/elements/to_json');
    }

    function mark_as_read() {
        $playerId = $this->params['form']['playerId'];
        $messageIds = array_key_exists('ids', $this->params['form']) ? $this->params['form']['ids'] : array();
        if (!is_array($messageIds)) {
            $messageIds = array($messageIds);
        }

        $this->Message->markAsRead($playerId, $messageIds);

        $data = array('success' => true);

        $this->set('data', $data);
        $this->render('/elements/to_json');
    }

    function listing() {
        $playerId = $this->params['form']['playerId'];
        $page = array_key_exists('page', $this->params['form']) ? intval($this->params['form']['page']) : 0;
        $folder = array_key_exists('folder', $this->params['form']) ? $this->params['form']['folder'] : "inbox";

        switch ($folder) {
//            case "trash":
//                $this->paginate = $this->Message->getTrash($playerId, $page);
//                break;
            case "sent":
                $this->paginate = $this->Message->getSent($playerId, $page);
                break;
            default: // Inbox
                $this->paginate = $this->Message->getInbox($playerId, $page);
                break;
        }

        // The view uses the playerId to determine whether the message is from/to this player
        $this->set('playerId', $playerId);
        $this->set('messages', $this->paginate('Message'));
    }

    function view($id = null) {
        $playerId = $this->params['form']['playerId'];
        $id = array_key_exists('id', $this->params['form']) ? intval($this->params['form']['id']) : -1;

        $message = $this->Message->getMessageForPlayer($playerId, $id);

        if (empty($message)) {
            $data = array('error' => 'The specified message could not be found');
            $this->set(compact('data'));
            $this->render('/elements/to_json');
            return;
        }
        
        // Set as read if necessary
        $refreshOnClose = false;
        if ($playerId == $message['Message']['recipient_player_id'] && $message['Message']['recipient_state'] == $this->Message->states['unread']) {
            $this->Message->markAsRead($playerId, $message['Message']['id']);
            $refreshOnClose = true;
        }

        $this->set('refreshOnClose', $refreshOnClose);
        $this->set('playerId', $playerId);
        $this->set(compact('message'));
    }

    function send() {
        $playerId = $this->params['form']['playerId'];
        $subject = $this->params['form']['subject'];
        $message = $this->params['form']['message'];
        $to = $this->params['form']['to'];

        // Only allow 1 message every x seconds
        $lastMessage = $this->Message->find('first', array(
                'contain' => array(),
                'field' => array('Message.created'),
                'conditions' => array(
                        'Message.sender_player_id' => $playerId
                ),
                'order' => 'Message.created DESC'
        ));

        if (!empty($lastMessage) && time() - strtotime($lastMessage['Message']['created']) < 10) {
            $data = array('error' => 'You are sending messages too fast. Please wait a few more seconds and try again.');
        } else {
            $data = $this->Message->send($playerId, $to, $subject, $message);
        }

        $this->set(compact('data'));
        $this->render('/elements/to_json');
    }
}