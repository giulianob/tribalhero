<?php

class DatabaseController extends AppController {
	var $name = 'Database';
	
	var $uses = array();
	
	var $unitStatRanges = array(
			"attack" => array('min' => 5, 'max' => 100),
			"defense" => array('min' => 5, 'max' => 84),
			"stealth" => array('min' => 0, 'max' => 14),
			"range" => array('min' => 0, 'max' => 19),
			"speed" => array('min' => 2, 'max' => 22),
			"carry" => array('min' => 0, 'max' => 352)
	);
	
	var $structureStatRanges = array(
			"defense" => array('min' => 0, 'max' => 5520),
			"stealth" => array('min' => 0, 'max' => 20),
			"range" => array('min' => 0, 'max' => 15)
	);		
	
	var $cacheAction = array(
		'index' => '1 month',
		'tree' => '1 month',
		'view' => '1 month'
	);
	
	function beforeFilter() {
		parent::beforeFilter();

		$this->Auth->allow(array('index', 'view', 'tree'));
	}
	
	function index() {
		$this->set('title_for_layout', 'Game Database');
	}
	
	function tree() {
		$this->set('title_for_layout', 'Building Tree');
	}	
	
	function view($key = null) {
		$this->set('structureStatRanges', $this->structureStatRanges);
		$this->set('unitStatRanges', $this->unitStatRanges);
		
		if (!preg_match("/^[a-zA-Z0-9_]*$/", $key)) {
			$this->render(false);
		} else {
			$this->render('generated/' . strtolower($key));
		}
	}
}