<?php

$paging = $paginator->params('Battle');

$results = array('pages' => $paging['pageCount'], 'page' => $paging['page'], 'snapshots' => array());

foreach($battle_reports as $battle_report) 
{
	$results['snapshots'][] = array(
		'id' => $battle_report['Battle']['id'],
		'date' => $time->niceShort($battle_report['Battle']['created']),
		'location' => $battle_report['City']['name'],
                'unread' => $battle_report['Battle']['read'] ? false : true
	);
}

echo json_encode($results);