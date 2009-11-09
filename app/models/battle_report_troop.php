<?php
class BattleReportTroop extends AppModel {

	var $name = 'BattleReportTroop';

	//The Associations below have been created with all possible keys, those that are not needed can be removed
	var $belongsTo = array(
		'BattleReport' => array(
			'className' => 'BattleReport',
			'foreignKey' => 'battle_report_id',
			'conditions' => '',
			'fields' => '',
			'order' => ''
		),
		'City' => array(
			'className' => 'City',
			'foreignKey' => 'city_id',
			'conditions' => '',
			'fields' => '',
			'order' => ''
		)
	);

	var $hasMany = array(
		'BattleReportObject' => array(
			'className' => 'BattleReportObject',
			'foreignKey' => 'battle_report_troop_id',
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

}
?>