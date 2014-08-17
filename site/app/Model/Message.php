<?php

class Message extends AppModel {

    var $name = 'Message';
    var $limitPerPage = 20;
    var $states = array(
        "unread" => 0,
        "read" => 1,
        "deleted" => 2,
        "permanent_delete" => 3
    );
    var $validate = array(
        'message' => array(
            'maxLength' => array(
                'rule' => array('maxLength', 30000),
                'message' => 'Message is too long. Shorten the message and try again.',
                'required' => true,
                'allowEmpty' => false
            )
        ),
        'subject' => array(
            'maxLength' => array(
                'rule' => array('maxLength', 70),
                'message' => 'Subject is too long. Shorten the subject and try again.',
                'required' => true,
                'allowEmpty' => false
            )
        )
    );
    var $belongsTo = array(
        'Sender' => array('className' => 'Player', 'foreignKey' => 'sender_player_id'),
        'Recipient' => array('className' => 'Player', 'foreignKey' => 'recipient_player_id')
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
            'order' => array('Message.created' => 'DESC')
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
            'order' => array('Message.created' => 'DESC')
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
                'NOT' => array('Message.sender_state' => $this->states['deleted'])
            ),
            'page' => $page,
            'limit' => $this->limitPerPage,
            'order' => array('Message.created' => 'DESC')
        );
    }

    function sendToTrash($playerId, $ids) {
        $this->updateAll(array(
            'Message.sender_state' => $this->states["deleted"]
                ), array(
            'Message.sender_player_id' => $playerId,
            'Message.id' => $ids
                )
        );

        $this->updateAll(array(
            'Message.recipient_state' => $this->states["deleted"]
                ), array(
            'Message.recipient_player_id' => $playerId,
            'Message.id' => $ids
                )
        );
    }

    function markAsRead($playerId, $ids) {
        $this->updateAll(array(
            'Message.recipient_state' => $this->states["read"]
                ), array(
            'Message.recipient_player_id' => $playerId,
            'Message.recipient_state' => $this->states["unread"],
            'Message.id' => $ids
                )
        );
    }

    function getMessageForPlayer($playerId, $messageId) {
        $message = $this->find('first', array(
                    'contain' => array(
                        'Recipient' => array('fields' => array('id', 'name')),
                        'Sender' => array('fields' => array('id', 'name'))
                    ),
                    'conditions' => array(
                        'Message.id' => $messageId,
                        'OR' => array(
                            array(
                                'Message.recipient_player_id' => $playerId,
                                'NOT' => array('Message.recipient_state' => $this->states['deleted'])
                            ),
                            array(
                                'Message.sender_player_id' => $playerId,
                                'NOT' => array('Message.sender_state' => $this->states['deleted'])
                            )
                        )
                    )
                ));

        if ($message['Sender']['id'] == null) {
            $message['Sender']['name'] = 'System';
        }

        return $message;
    }

    function getUnreadCount($playerId) {
        return $this->find('count', array(
            'contain' => array(),
            'conditions' => array(
                'Message.recipient_player_id' => $playerId,
                'Message.recipient_state' => $this->states['unread']
            )
        ));
    }

    function send($playerId, $recipient, $subject, $message) {
        if ($recipient['Recipient']['id'] == $playerId) {
            return array('error' => 'You aren\'t allowed to send messages to yourself.');
        }

        $newMessage = array(
            'sender_player_id' => $playerId,
            'recipient_player_id' => $recipient['Recipient']['id'],
            'subject' => $subject,
            'message' => $message
        );

        if ($this->save($newMessage, true, array('sender_player_id', 'recipient_player_id', 'subject', 'message'))) {
            return array('success' => true);
        } else {
            $errors = $this->invalidFields();
            return array('error' => reset($errors));
        }
    }

}
