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
	
	function admin_battle_stats() {
	
		// Aggregate of unit stats
		$BattleReportObject =& ClassRegistry::init('BattleReportObject');
		
		$stats = $BattleReportObject->find('all', array(
			'link' => array(
				'BattleReportTroop' => array(
					'type' => 'INNER',
					'conditions' => array(
						'BattleReportTroop.state' => array(TROOP_STATE_EXITING, TROOP_STATE_DYING, TROOP_STATE_RETREATING, TROOP_STATE_OUT_OF_STAMINA)
					),
					'fields' => array()
				)
			),
			'fields' => array(
				'BattleReportObject.type', 
				'BattleReportObject.level', 
				'BattleReportObject.formation_type', 
				'AVG(IF(hits_dealt_by_unit=0,0,damage_dealt/hits_dealt_by_unit)) as damage_average', 
				'MAX(IF(hits_dealt_by_unit=0,0,damage_dealt/hits_dealt_by_unit)) as damage_max', 
				'AVG(damage_taken) as damage_taken_average', 
				'AVG(hits_dealt) as hits_dealt_average', 
				'AVG(count) as count_average'
			),
			'group' => array('BattleReportObject.type', 'BattleReportObject.level', 'BattleReportObject.formation_type'),
			'order' => array('BattleReportObject.type', 'BattleReportObject.formation_type', 'BattleReportObject.level'),
		));				
		
		$this->set('stats', $stats);
		
		// Top player by troop size
		$TroopStubList =& ClassRegistry::init('TroopStubList');
		
		$troopSizeStats = $TroopStubList->find('all', array(				
			'link' => array(
				'City' => array(
					'fields' => array('City.name')
				)
			),
			'order' => array('troop_count' => 'desc'),
			'fields' => array('SUM(count) as troop_count'),
			'group' => array('TroopStubList.city_id'),
			'limit' => 50
		));
		
		$this->set('troopSizeStats', $troopSizeStats);
		
		// Top player by resources
		$City =& ClassRegistry::init('City');
		
		$resourceStats = $City->find('all', array(				
			'link' => array(),
			'order' => array('resource_total' => 'desc'),
			'fields' => array('name', 'gold', 'wood', 'crop', 'iron', 'labor', '(gold+wood+crop+iron+labor) as resource_total'),
			'limit' => 50
		));
		
		$this->set('resourceStats', $resourceStats);		
	}
}