<?php
class Battle extends AppModel {

	var $name = 'Battle';
	var $actsAs = 'Containable';
	
	//The Associations below have been created with all possible keys, those that are not needed can be removed
	var $belongsTo = array(
		'City' => array(
			'className' => 'City',
			'foreignKey' => 'city_id',
			'conditions' => '',
			'fields' => '',
			'order' => ''
		)
	);
	
	var $hasMany = array(
		'BattleReport' => array(
			'className' => 'BattleReport',
			'foreignKey' => 'battle_id',
			'dependent' => false,
			'conditions' => 'BattleReport.ready = 1',
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