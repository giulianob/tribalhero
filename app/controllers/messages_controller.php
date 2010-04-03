<?php

class MessagesController extends AppController {
    var $helpers = array('Time', 'Text');    
    var $allowedFromGame = array('listing', 'view');

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

    function listing() {
        $playerId = $this->params['form']['playerId'];
        $page = array_key_exists('page', $this->params['form']) ? intval($this->params['form']['page']) : 0;
        $folder = array_key_exists('folder', $this->params['form']) ? $this->params['form']['folder'] : "inbox";
        
        switch ($folder) {
            case "trash":
                $this->paginate = $this->Message->getTrash($playerId, $page);
                break;
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
        $message = $this->Message->find('first', array(
                'contain' => array(
                        'Recipient' => array('fields' => array('id', 'name')),
                        'Sender' => array('fields' => array('id', 'name'))
                ),
                'conditions' => array(
                        'OR' => array(
                                array(
                                        'Message.recipient_player_id' => $this->Session->read('Auth.Player.id'),
                                        'NOT' => array('Message.recipient_state' => $this->Message->states['deleted'])
                                ),
                                array(
                                        'Message.sender_player_id' => $this->Session->read('Auth.Player.id'),
                                        'NOT' => array('Message.sender_state' => $this->Message->states['deleted'])
                                )
                        )
                )
        ));

        $this->set(compact('message'));
    }
}