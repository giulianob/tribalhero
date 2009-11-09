<?php
class Message extends AppModel {

	var $name = 'Message';
	var $actsAs = array('Containable', 'Linkable');
	
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
}
?>