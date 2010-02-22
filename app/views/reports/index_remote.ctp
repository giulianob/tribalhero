<?php

$paging = $paginator->params('BattleReport');

$results = array('pages' => $paging['pageCount'], 'page' => $paging['page'], 'snapshots' => array());

foreach($battle_reports as $battle_report) 
{
	$results['snapshots'][] = array(
		'id' => $battle_report['BattleReport']['id'],
		'date' => $battle_report['BattleReport']['created'],
		'location' => $battle_report['City']['name'],
		'troop' => $battle_report['TroopCity']['name'] . '(' . $battle_report['BattleReportTroopEnter']['troop_stub_id'] . ')',
		'side' => $battle_report['BattleReportTroopEnter']['is_attacker']?'Attack':'Defense'
	);
}

echo json_encode($results);