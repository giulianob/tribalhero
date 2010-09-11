<?php
class Player extends AppModel {
	
	var $name = 'Player';
	
	var $actsAs = array('Containable');
	
	var $validate = array(
		'name' => array(
			'between' => array(
				'rule' => array('between', 5, 32),
				'message' => 'Name must be between 5 and 32 characters in length',
				'required' => true
			),
			'alphanumeric' => array(
				'rule' => 'alphaNumeric',
				'message' => 'Player name must only contain letters and numbers'
			),
			'unique' => array(
				'rule' => 'isUnique',
				'message' => 'A player with this name already exists'
			),
			'notSystem' => array(
				'rule' => '/^((?!system).)*$/i',
				'message' => 'Illegal player name. Player names cannot contain the word "System"'
			)
		), 
		'email_address' => array(
			'unique' => array(
				'rule' => 'isUnique',
				'message' => 'This email address has already been registered',
				'required' => true,
				'allowEmpty' => false
			),
			'between' => array(
				'rule' => array('between', 5, 256),
				'message' => 'Must be between 5 and 256 characters long'
			),
			'email' => array(
				'rule' => array('email', false),
				'message' => 'This is not a valid email address'
			)
		),
		'password_once' => array(
			'match' => array(
				'rule' => array('match', 'password_twice'),
				'message' => 'Passwords must match'
			),
			'between' => array(
				'rule' => array('between', 5, 32),
				'message' => 'Must be between 5 and 32 characters long',
				'on' => 'create',
				'required' => true
			),
			'betweenUpdt' => array(
				'rule' => array('between', 5, 32),
				'message' => 'Must be between 5 and 32 characters long',
				'on' => 'update',
				'required' => false,
				'allowEmpty' => true
			)			
		)
	);		
			
	var $hasOne = array(
		'City'
	);		
	
	var $hasMany = array(
        'SendMessage' => array(
            'className'     => 'Message',
            'foreignKey'    => 'sender_player_id',
            'conditions'    => array('Message.sender_state !=' => '2'),
            'order'    		=> 'Message.created DESC',
            'limit'        	=> '30',
            'dependent'		=> true
        ),
        'ReceiveMessage' => array(
            'className'     => 'Message',
            'foreignKey'    => 'recipient_player_id',
            'conditions'    => array('Message.recipient_state !=' => '2'),
            'order'    		=> 'Message.created DESC',
            'limit'        	=> '30',
            'dependent'		=> true
        )
	);	
	
	function match($field, $matchField){
		$val = array_pop($field);
		return empty($val) || strcmp($val, $this->data[$this->name][$matchField]) == 0;
	}	
}
?>