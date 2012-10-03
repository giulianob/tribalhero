<?php

class Player extends AppModel {

    var $name = 'Player';
    var $hasOne = array(
        'City',
        'Tribesman'
    );
    var $hasMany = array(
        'SendMessage' => array(
            'className' => 'Message',
            'foreignKey' => 'sender_player_id',
            'conditions' => array('Message.sender_state !=' => '2'),
            'order' => 'Message.created DESC',
            'limit' => '30',
            'dependent' => true
        ),
        'ReceiveMessage' => array(
            'className' => 'Message',
            'foreignKey' => 'recipient_player_id',
            'conditions' => array('Message.recipient_state !=' => '2'),
            'order' => 'Message.created DESC',
            'limit' => '30',
            'dependent' => true
        )
    );

    public function getTribeId($playerId) {
        $tribesman = $this->Tribesman->findByPlayerId($playerId);
        if (empty($tribesman))
            return null;

        return $tribesman['Tribesman']['tribe_id'];
    }

}