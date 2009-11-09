<?php

class MessagesController extends AppController
{
	var $helpers = array('Time', 'Text');
	var $limitPerPage = 25;
	
	function beforeFilter()
	{
		$this->layout = 'ajax';
		
		parent::beforeFilter();
	}	
	
	function index()
	{				
		$this->paginate = array(
			'contain' => array(
				'Sender' => array('fields' => array('id', 'name'))
			),
			'conditions' => array(
				'Message.recipient_player_id' => $this->Session->read('Auth.Player.id'),
				'NOT' => array('Message.recipient_state' => $this->Message->states['deleted'])			
			),
			'limit' => $this->limitPerPage,
			'order' => 'Message.created'
		);
		
		$this->set('messages', $this->paginate('Message'));
	}
	
	function view($id = null)
	{
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