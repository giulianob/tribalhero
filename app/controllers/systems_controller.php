<?php

class SystemsController extends AppController {

	var $uses = array();
	
	var $helpers = array('Number');

	var $name = 'Systems';
	
	function admin_status() {
		$SystemVariable =& ClassRegistry::init('SystemVariable');
		
		$variables = $SystemVariable->find('all');
		
		$Player =& ClassRegistry::init('Player');
		
		$onlinePlayers = $Player->find('all', array(
			'contains' => array(),
			'conditions' => array('Player.online' => true)
		));
		
		$this->set('onlinePlayers', $onlinePlayers);
		$this->set('variables', $variables);
	}
	
	function admin_clear_cache() {
		Cache::clear();
		clearCache();
		
		$this->set('data', array('msg' => 'Cache cleared'));
		$this->render('/elements/to_json');
	}
}