<?php
class Message extends AppModel {

    var $name = 'Message';
    var $actsAs = array('Containable', 'Linkable');

    var $limitPerPage = 25;

    var $states = array(
            "unread" => 0,
            "read" => 1,
            "deleted" => 2
    );

    var $validate = array(
            'message' => array(
                            'maxLength' => array(
                                            'rule' => array('maxLength', 1000),
                                            'message' => 'The maximum message length is 1000 characters. Shorten the message and try again.',
                                            'required' => true,
                                            'allowEmpty' => false
                            )
            ),
            'subject' => array(
                            'maxLength' => array(
                                            'rule' => array('maxLength', 150),
                                            'message' => 'The maximum subject length is 150 characters. Shorten the subject and try again.',
                                            'required' => true,
                                            'allowEmpty' => false
                            )
            )
    );

    var $belongsTo = array(
            'Sender' => array('className' => 'Player',
                            'foreignKey' => 'sender_player_id',
                            'conditions' => '',
                            'fields' => '',
                            'order' => '',
            ),
            'Recipient' => array('className' => 'Player',
                            'foreignKey' => 'recipient_player_id',
                            'conditions' => '',
                            'fields' => '',
                            'order' => ''
            )
    );

    function getInbox($playerId, $page) {
        return array(
                'contain' => array(
                        'Sender' => array('fields' => array('id', 'name')),
                        'Recipient' => array('fields' => array('id', 'name'))
                ),
                'conditions' => array(
                        'Message.recipient_player_id' => $playerId,
                        'NOT' => array('Message.recipient_state' => $this->states['deleted'])
                ),
                'page' => $page,
                'limit' => $this->limitPerPage,
                'order' => 'Message.created'
        );
    }

    function getTrash($playerId, $page) {
        return array(
                'contain' => array(
                        'Sender' => array('fields' => array('id', 'name')),
                        'Recipient' => array('fields' => array('id', 'name'))
                ),
                'conditions' => array(
                        'OR' => array(
                                array(
                                        'Message.recipient_player_id' => $playerId,
                                        'Message.recipient_state' => $this->states['deleted']
                                ),
                                array(
                                        'Message.sender_player_id' => $playerId,
                                        'Message.sender_state' => $this->states['deleted']
                                )
                        )
                ),
                'page' => $page,
                'limit' => $this->limitPerPage,
                'order' => 'Message.created'
        );
    }

    function getSent($playerId, $page) {
        return array(
                'contain' => array(
                        'Sender' => array('fields' => array('id', 'name')),
                        'Recipient' => array('fields' => array('id', 'name'))
                ),
                'conditions' => array(
                        'Message.sender_player_id' => $playerId,
                        'NOT' => array('Message.recipient_state' => $this->states['deleted'])
                ),
                'page' => $page,
                'limit' => $this->limitPerPage,
                'order' => 'Message.created'
        );
    }
}
