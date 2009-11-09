<?php

class PagesController extends AppController
{
	var $uses = array();
	
	function beforeFilter()
	{
		parent::beforeFilter();		
		$this->Auth->allow(array('display'));
	}		
	
	function display() {
		$path = func_get_args();
		
		$count = count($path);
		if (!$count) {
			$this->redirect('/');
		}
		
		$page = $subpage = $title = null;

		if (!empty($path[0])) {
			$page = $path[0];
		}
		if (!empty($path[1])) {
			$subpage = $path[1];
		}
		if (!empty($path[$count - 1])) {
			$title = Inflector::humanize($path[$count - 1]);
		}
		$this->set(compact('page', 'subpage', 'title'));
		$this->render(join('/', $path));
	}	
	
	function play() {
		$this->pageTitle = 'Play';
		$this->layout = 'game';
		
		$loginKey = sha1($this->Auth->user('id') + '.' + time() + '.' + rand(0,1000));
			
		$player =& ClassRegistry::init('Player');
				
		if (!$player->save(array(
			'id' => $this->Auth->user('id'),
			'login_key_date' => date('Y-m-d H:i:s'),
			'login_key' => $loginKey
		), false))
		{
			$this->Session->setFlash('We were unable to log you into the world. Refresh this page to try again, if the problem persists, contact us.');
		}
		else
		{
			$this->set('lsessid', $loginKey);
		}
	}
	
	function game_popup()
	{		
		Configure::write('debug', 0);
		$this->layout = 'popup';
	}
}
