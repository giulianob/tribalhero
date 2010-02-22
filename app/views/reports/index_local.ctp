<?php

$paging = $paginator->params('Battle');

$results = array('pages' => $paging['pageCount'], 'page' => $paging['page'], 'snapshots' => array());

foreach($battle_reports as $battle_report) 
{
	$results['snapshots'][] = array(
		'id' => $battle_report['Battle']['id'],
		'date' => $battle_report['Battle']['created'],
		'location' => $battle_report['City']['name'],
	);
}

echo json_encode($results);