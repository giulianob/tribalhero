<?php

$paging = $paginator->params('BattleReportView');

$results = array('pages' => $paging['pageCount'], 'page' => $paging['page'], 'snapshots' => array());

foreach($battle_reports as $battle_report) 
{
	$results['snapshots'][] = array(
		'id' => $battle_report['BattleReportView']['id'],
		'date' => $battle_report['BattleReportView']['created'],
		'location' => $battle_report['City']['name'],
		'troop' => $battle_report['TroopCity']['name'] . '(' . $battle_report['BattleReportView']['troop_stub_id'] . ')',
		'side' => $battle_report['BattleReportView']['is_attacker']?'Attack':'Defense'
	);
}

echo json_encode($results);