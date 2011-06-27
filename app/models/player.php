<?php

class Player extends AppModel {

    var $name = 'Player';
    var $validate = array(
        'name' => array(
            array('rule' => 'notEmpty', 'required' => true, 'on' => 'create'),
            array('rule' => array('between', 5, 32), 'message' => 'Name must be between 5 and 32 characters in length'),
            array('rule' => 'alphaNumeric', 'message' => 'Player name must only contain letters and numbers'),
            array('rule' => 'isUnique', 'message' => 'A player with this name already exists'),
            array('rule' => '/^((?!system).)*$/i', 'message' => 'Illegal player name. Player names cannot contain the word "System"'),
        ),
        'email_address' => array(
            array('rule' => 'notEmpty', 'required' => true, 'on' => 'create'),
            array('rule' => 'isUnique', 'message' => 'This email address has already been registered'),
            array('rule' => array('email', false), 'message' => 'This is not a valid email address'),
        ),
        'password_once' => array(
            array('rule' => 'notEmpty', 'required' => true, 'on' => 'create'),
            array('rule' => array('match', 'password_twice'), 'message' => 'Passwords must match'),
            array('rule' => array('between', 5, 32), 'message' => 'Must be between 5 and 32 characters long'),
        )
    );
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