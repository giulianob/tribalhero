<?php

$paging = $paginator->params('Battle');

$results = array('pages' => $paging['pageCount'], 'page' => $paging['page'], 'snapshots' => array());

foreach ($battle_reports as $battle_report) {
    $defenderTribes = array();
    $attackerTribes = array();

    foreach ($battle_report['Tribe'] as $tribe) {
        $arr = $tribe['BattleTribe']['is_attacker'] ? $attackerTribes : $defenderTribes;
        array_push($arr, array('id' => $tribe['id'], 'name' => $tribe['name']));
    }

    $results['snapshots'][] = array(
        'id' => $battle_report['Battle']['id'],
        'date' => $time->niceShort($battle_report['Battle']['created']),
        'location' => $battle_report['Battle']['location_name'],
        'unread' => $battle_report['Battle']['read'] ? false : true,
        'defenderTribes' => $defenderTribes,
        'attackerTribes' => $attackerTribes
    );
}

echo json_encode($results);