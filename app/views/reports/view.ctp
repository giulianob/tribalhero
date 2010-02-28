<?php

$results = array('outcomeOnly' => false, 'snapshots' => array());

if (count($battle_reports) > 0)
    $battleStartTime = strtotime($battle_reports[0]['BattleReport']['created']);

foreach($battle_reports as $battle_report) {
    $snapshot = array('round' => $battle_report['BattleReport']['round'], 'turn' => $battle_report['BattleReport']['turn'], 'time' => strtotime($battle_report['BattleReport']['created']) - $battleStartTime, 'events' => array(), 'attackers' => array(), 'defenders' => array());

    foreach($battle_report['BattleReportTroop'] as $battle_troop) {
        //Save the events
        if ($battle_troop['state'] != TROOP_STATE_STAYING) {
            $snapshot['events'][] = $battle_troop['City']['name'].'('.($battle_troop['troop_stub_id']==1?'Local':$battle_troop['troop_stub_id']).') has '.$troop_states_pst[$battle_troop['state']];
        }

        //Save the main troop info
        $troop = array('cityId' => $battle_troop['City']['id'], 'name' => $battle_troop['City']['name'].'('.($battle_troop['troop_stub_id']==1?'Local':$battle_troop['troop_stub_id']).')', 'units' => array());

        //Only attackers have resources
        if ($battle_troop['is_attacker'])
            $troop['resources'] = array('gold' => $battle_troop['gold'], 'crop' => $battle_troop['crop'], 'iron' => $battle_troop['iron'], 'wood' => $battle_troop['wood']);

        //Gather all the unit info
        foreach($battle_troop['BattleReportObject'] as $battle_object) {
            $troop['units'][] = array(
                    'type' => $battle_object['type'],
                    'level' => $battle_object['level'],
                    'count' => $battle_object['count'],
                    'hp' => $battle_object['hp'],
                    'dmgTaken' => $battle_object['damage_taken'],
                    'dmgDealt' => $battle_object['damage_dealt']
            );
        }

        //Save to appropriate location
        if ($battle_troop['is_attacker'])
            $snapshot['attackers'][] = $troop;
        else
            $snapshot['defenders'][] = $troop;
    }

    $results['snapshots'][] = $snapshot;
}


echo json_encode($results);