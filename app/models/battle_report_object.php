<?php
class BattleReportObject extends AppModel {

	var $name = 'BattleReportObject';	
	
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