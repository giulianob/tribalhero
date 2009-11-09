<?php
class BattleReport extends AppModel {

	var $name = 'BattleReport';
	var $actsAs = array('Containable', 'Linkable');
	var $order = array('BattleReport.created DESC');

	//The Associations below have been created with all possible keys, those that are not needed can be removed
	var $belongsTo = array(
		'Battle' => array(
			'className' => 'Battle',
			'foreignKey' => 'battle_id',
			'conditions' => '',
			'fields' => '',
			'order' => ''
		)
	);
	
	var $hasMany = array(
		'BattleReportTroop' => array(
			'className' => 'BattleReportTroop',
			'foreignKey' => 'battle_report_id',
			'dependent' => false,
			'conditions' => '',
			'fields' => '',
			'order' => '',
			'limit' => '',
			'offset' => '',
			'exclusive' => '',
			'finderQuery' => '',
			'counterQuery' => ''
		)
	);
	

	function paginateCount($conditions = array(), $recursive = 0, $extra = array()) {
        $parameters = compact('conditions');
        if ($recursive != $this->recursive) {
                $parameters['recursive'] = $recursive;
        }
        
        unset($extra['link']);
        return $this->find('count', array_merge($parameters, $extra));       
	}
	

}