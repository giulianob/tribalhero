<?php

$results = array('refreshOnClose' => $refresh_on_close, 'outcomeOnly' => false, 'snapshots' => array(), 'groupId' => (array_key_exists('BattleReportView', $main_report) ? $main_report['BattleReportView']['group_id'] : 1));

// Set resources gained if available
if (array_key_exists('BattleReportView', $main_report)) {
    $results['loot'] = array(
        'crop' => $main_report['BattleReportView']['loot_crop'],
        'wood' => $main_report['BattleReportView']['loot_wood'],
        'iron' => $main_report['BattleReportView']['loot_iron'],
        'gold' => $main_report['BattleReportView']['loot_gold']
    );
    $results['bonus'] = array(
        'crop' => $main_report['BattleReportView']['bonus_crop'],
        'wood' => $main_report['BattleReportView']['bonus_wood'],
        'iron' => $main_report['BattleReportView']['bonus_iron'],
        'gold' => $main_report['BattleReportView']['bonus_gold']
    );
}

if (count($battle_reports) > 0)
    $battleStartTime = strtotime($battle_reports[0]['BattleReport']['created']);

foreach ($battle_reports as $battle_report) {
    $snapshot = array('round' => $battle_report['BattleReport']['round'], 'turn' => $battle_report['BattleReport']['turn'], 'time' => strtotime($battle_report['BattleReport']['created']) - $battleStartTime, 'events' => array(), 'attackers' => array(), 'defenders' => array());

    foreach ($battle_report['BattleReportTroop'] as $battle_troop) {
        //Save the events
        if ($battle_troop['state'] != TROOP_STATE_STAYING) {
            $snapshot['events'][] = $battle_troop['City']['name'] . '(' . ($battle_troop['troop_stub_id'] == 1 ? 'Local' : $battle_troop['troop_stub_id']) . ') has ' . $troop_states_pst[$battle_troop['state']];
			$snapshot['eventsRaw'][] = array('groupId' => $battle_troop['group_id'], 'type' => $battle_troop['state']);
        }

        //Save the main troop info
        $troop = array('cityId' => $battle_troop['City']['id'], 'groupId' => $battle_troop['group_id'], 'name' => $battle_troop['City']['name'] . '(' . ($battle_troop['troop_stub_id'] == 1 ? 'Local' : $battle_troop['troop_stub_id']) . ')', 'units' => array());

        //Only attackers have resources
        if ($battle_troop['is_attacker'])
            $troop['resources'] = array('gold' => $battle_troop['gold'], 'crop' => $battle_troop['crop'], 'iron' => $battle_troop['iron'], 'wood' => $battle_troop['wood']);

        //Gather all the unit info
        foreach ($battle_troop['BattleReportObject'] as $battle_object) {
            $troop['units'][] = array(
				'id' => $battle_object['object_id'],
                'type' => $battle_object['type'],
                'level' => $battle_object['level'],
                'count' => $battle_object['count'],
                'hp' => $battle_object['hp'],
                'dmgTaken' => $battle_object['damage_taken'],
                'dmgDealt' => $battle_object['damage_dealt'],
                'hitsTaken' => $battle_object['hits_received'],
                'hitsDealt' => $battle_object['hits_dealt'],
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