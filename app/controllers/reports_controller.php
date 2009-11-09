<?php

class ReportsController extends AppController
{
	var $uses = array();
	var $helpers = array('TimeAdv');	
	
	//temp until i figure out how to put this in bootstrap
	var	$troop_states_pst = array(
    	'joined the battle',
    	'stayed',
    	'left the battle',
    	'died',
    	'retreated back to their city',
    	'trained new units'
	);	
	
	var $object_types = array(
		101 => 'Swordsman',
		102 => 'Hoplite',
		103 => 'Archer',
		201 => 'Calvary',
		202 => 'Heavy Calvary',
		302 => 'Helepolis',
		2000 => 'Town',
		2102 => 'Refinery',
		2106 => 'Farm',
		2107 => 'Lumbermill',
		2108 => 'Advanced Farm',
		2109 => 'Advanced Lumbermill',
		2110 => 'Foundry',
		2201 => 'Barrack',
		2202 => 'Stable',
		2203 => 'Workshop',
		2301 => 'Academy',
		2302 => 'Armory',
		2303 => 'Blacksmith',
		2304 => 'Cannonsmith',
		2305 => 'Gunsmith',
		2306 => 'Whitesmith',
		2402 => 'Towwer',
		2403 => 'Cannon Tower',
		2501 => 'Distribution Center',
		2502 => 'Market',
		2503 => 'Embassy',
		2504 => 'Shop',
		2000 => 'Town',
	);
	
	var $formations = array(
        1 => "Normal",
        2 => "Attack",
        3 => "Defense",
        4 => "Scout",
        5 => "Garrison",
        6 => "Structure",
        7 => "Local",
        11 => "Captured",
        12 => "Wounded",
        13 => "Killed"
	);
	
	function beforeFilter()
	{
		$this->layout = 'ajax';
		
		parent::beforeFilter();
	}	
	
	function index()
	{
		$this->City =& ClassRegistry::init('City');
		
		$this->City->recursive = -1;		
		
		$cities = $this->City->find('list', array('conditions' => array(
			'player_id' => $this->Auth->user('id')
		)));						
		
		$this->BattleReport =& ClassRegistry::init('BattleReport');
		
		$this->paginate = array(
	 		'joins' => array(
	 			array(	 		
		            'table' => 'battle_report_troops',
		            'alias' => 'BattleReportTroopEnter',
		            'type' => 'INNER',
		            'foreignKey' => false,
		            'conditions'=> array(
						'BattleReportTroopEnter.battle_report_id = BattleReport.id',
						'BattleReportTroopEnter.state' => TROOP_STATE_ENTERING,
						'BattleReportTroopEnter.city_id' => array_keys($cities)
					)
		        ),
	 			array(	 		
		            'table' => 'battle_report_troops',
		            'alias' => 'BattleReportTroopExit',
		            'type' => 'INNER',
		            'foreignKey' => false,
		            'conditions'=> array(
						'BattleReportTroopExit.group_id = BattleReportTroopEnter.group_id',
						'AND' => array('OR' => array(
							array('BattleReportTroopExit.state' => TROOP_STATE_RETREATING),
							array('BattleReportTroopExit.state' => TROOP_STATE_EXITING),
							array('BattleReportTroopExit.state' => TROOP_STATE_DYING)
						))
					)
		        ),
				array(
		            'table' => 'battle_reports',
		            'alias' => 'BattleReportExit',
		            'type' => 'INNER',
		            'foreignKey' => false,
		            'conditions'=> array(
						'BattleReportExit.id = BattleReportTroopExit.battle_report_id',
						'BattleReportExit.battle_id = BattleReport.battle_id',
						'BattleReportExit.ready = 1',
					)
		        ),
	 			array(	 		
		            'table' => 'cities',
		            'alias' => 'TroopCity',
		            'type' => 'INNER',
		            'foreignKey' => false,
		            'conditions'=> array(
						'BattleReportTroopEnter.city_id = TroopCity.id',
					)
		        )	        
	        ), 		
	        'fields' => array(
	        	'BattleReport.id',
	        	'BattleReport.battle_id',
	        	'BattleReport.round',
	        	'BattleReport.turn',	      
	        	'BattleReport.created',  	
	        	'BattleReportTroopEnter.is_attacker',
	        	'TroopCity.name'
	        ),
			'conditions' => array(			
				'BattleReport.ready' => '1'
			),
			'link' => array(
				'Battle' => array(
					'City' => array(
						'fields' => array('name')
					)
				)
			),
			'recursive' => '-1',
			'limit' => '15'
		);
		
		$reports = $this->paginate('BattleReport');
		
		$this->set('battle_reports', $reports);
	}
	
	function view($id = null)
	{
		if (empty($id))
			$this->redirect(array('action' => 'reports'));
			
		$this->City =& ClassRegistry::init('City');
		
		$this->City->recursive = -1;		
		
		$cities = $this->City->find('list', array('conditions' => array(
			'player_id' => $this->Auth->user('id')
		)));						
		
		//main report data to find the beginning/end of the battle reports we need
		$report = $this->City->Battle->BattleReport->find('first', array(
	 		'joins' => array(
	 			array(	 		
		            'table' => 'battle_report_troops',
		            'alias' => 'BattleReportTroopEnter',
		            'type' => 'INNER',
		            'foreignKey' => false,
		            'conditions'=> array(
						'BattleReportTroopEnter.battle_report_id = BattleReport.id',
						'BattleReportTroopEnter.state' => TROOP_STATE_ENTERING,
						'BattleReportTroopEnter.city_id' => array_keys($cities)
					)
		        ),
	 			array(
		            'table' => 'battle_report_troops',
		            'alias' => 'BattleReportTroopExit',
		            'type' => 'INNER',
		            'foreignKey' => false,
		            'conditions'=> array(
						'BattleReportTroopExit.group_id = BattleReportTroopEnter.group_id',
						'AND' => array('OR' => array(
							array('BattleReportTroopExit.state' => TROOP_STATE_RETREATING),
							array('BattleReportTroopExit.state' => TROOP_STATE_EXITING),
							array('BattleReportTroopExit.state' => TROOP_STATE_DYING)
						))
					)
		        ),
				array(
		            'table' => 'battle_reports',
		            'alias' => 'BattleReportExit',
		            'type' => 'INNER',
		            'foreignKey' => false,
		            'conditions'=> array(
						'BattleReportExit.id = BattleReportTroopExit.battle_report_id',
						'BattleReportExit.battle_id = BattleReport.battle_id',
						'BattleReportExit.ready = 1',
					)
		        ),
	 			array(	 		
		            'table' => 'cities',
		            'alias' => 'TroopCity',
		            'type' => 'INNER',
		            'foreignKey' => false,
		            'conditions'=> array(
						'BattleReportTroopEnter.city_id = TroopCity.id',
					)
		        )	        
	        ), 		
	        'fields' => array(
	        	'BattleReport.id',
	        	'BattleReport.battle_id',
	        	'BattleReport.round',
	        	'BattleReport.turn',	      
	        	'BattleReport.created',
	        	'BattleReportExit.round',
	        	'BattleReportExit.turn',	        
	        	'TroopCity.name'
	        ),
			'conditions' => array(			
				'BattleReport.id' => $id,
				'BattleReport.ready' => '1'
			),
			'link' => array(
				'Battle' => array(
					'fields' => array('id'),
					'City' => array(
						'fields' => array('name')
					)
				)
			),
			'recursive' => '-1'
		));
		
		if (empty($report))
		{
			$this->Session->setFlash('Invalid battle report specified.');
			$this->redirect(array('action' => 'reports'));			
		}				
			
		$this->paginate = array(
			'conditions' => array(
				'BattleReport.battle_id' => $report['Battle']['id'],
				'OR' => array(
					array('BattleReport.round' => $report['BattleReport']['round'], 'BattleReport.turn >=' => $report['BattleReport']['turn']),
					array('BattleReport.round >' => $report['BattleReport']['round'], 'BattleReport.round <' => $report['BattleReportExit']['round']),
					array('BattleReport.round' => $report['BattleReportExit']['round'], 'BattleReport.turn <=' => $report['BattleReportExit']['turn'])				
				)
			),
			'contain' => array(
				'BattleReportTroop' => array(
					'City' => array('fields' => array('id', 'name')), 
					'BattleReportObject' => array('order' => array('BattleReportObject.formation_type ASC', 'BattleReportObject.type ASC'))
				)
			),
			'order' => array(				
				'BattleReport.round ASC',
				'BattleReport.turn ASC'
			),
			'limit' => 30
		);		
		
		$reports = $this->paginate($this->City->Battle->BattleReport);		
		$this->set('formations', $this->formations);
		$this->set('troop_states_pst', $this->troop_states_pst);
		$this->set('main_report', $report);
		$this->set('battle_reports', $reports);
		$this->set('object_types', $this->object_types);
	}
}