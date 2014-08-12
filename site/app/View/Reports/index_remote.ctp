<?php

$paging = $this->Paginator->params('Battle');

$results = array('pages' => $paging['pageCount'], 'page' => $paging['page'], 'snapshots' => array());

foreach ($battle_reports as $battle_report) {
    $defenderTribes = array();
    $attackerTribes = array();

    foreach ($battle_report['Tribe'] as $tribe) {
        if ($tribe['BattleTribe']['is_attacker']) {
            $attackerTribes[] = array('id' => $tribe['id'], 'name' => $tribe['name']);
        }
        else {
            $defenderTribes[] = array('id' => $tribe['id'], 'name' => $tribe['name']);
        }
    }

    $results['snapshots'][] = array(
        'id' => $battle_report['BattleReportView']['id'],
        'date' => strtotime($battle_report['Battle']['ended']),
        'location' => array(
            'type' => $battle_report['Battle']['location_type'],
            'id' => $battle_report['Battle']['location_id'],
            'name' => $battle_report['Battle']['location_name']
        ),
        'troop' => $battle_report['BattleReportView']['owner_name'] . ($battle_report['BattleReportView']['troop_stub_id'] == 0 ? '' : ('('.$battle_report['BattleReportView']['troop_stub_id'].')')),
        'side' => $battle_report['BattleReportView']['is_attacker'] ? 'Attack' : 'Defense',
        'unread' => $battle_report['BattleReportView']['read'] ? false : true,
        'defenderTribes' => $defenderTribes,
        'attackerTribes' => $attackerTribes
    );
}

echo json_encode($results);