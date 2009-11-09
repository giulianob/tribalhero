<?php
class BattleReportObject extends AppModel {

	var $name = 'BattleReportObject';	
	
	//The Associations below have been created with all possible keys, those that are not needed can be removed
	var $belongsTo = array(
		'BattleReportTroop' => array(
			'className' => 'BattleReportTroop',
			'foreignKey' => 'battle_report_troop_id',
			'conditions' => '',
			'fields' => '',
			'order' => ''
		)
	);

}
?>