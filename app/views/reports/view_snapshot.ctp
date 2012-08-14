<?php

$snapshot = array('round' => $battleReport['BattleReport']['round'], 'turn' => $battleReport['BattleReport']['turn'], 'time' => strtotime($battleReport['BattleReport']['created']) - strtotime($battleStartTime), 'events' => array(), 'attackers' => array(), 'defenders' => array());

foreach ($battleReport['BattleReportTroop'] as $battleTroop) {
    //Save the main troop info
    $troop = array('name' => $battleTroop['name'], 'groupId' => $battleTroop['group_id'], 'owner' => $battleTroop['owner'], 'units' => array());

    //Only attackers have resources
    if ($battleTroop['is_attacker'])
        $troop['resources'] = array('gold' => $battleTroop['gold'], 'crop' => $battleTroop['crop'], 'iron' => $battleTroop['iron'], 'wood' => $battleTroop['wood']);

    //Gather all the unit info
    foreach ($battleTroop['BattleReportObject'] as $battle_object) {
        $troop['units'][] = array(
            'id' => $battle_object['object_id'],
            'type' => $battle_object['type'],
            'level' => $battle_object['level'],
            'count' => $battle_object['count'],
            'hp' => $battle_object['hp'],
            'damageTaken' => $battle_object['damage_taken'],
            'damageDealt' => $battle_object['damage_dealt'],
            'hitsTaken' => $battle_object['hits_received'],
            'hitsDealt' => $battle_object['hits_dealt'],
        );
    }

    //Save to appropriate location
    if ($battleTroop['is_attacker'])
        $snapshot['attackers'][] = $troop;
    else
        $snapshot['defenders'][] = $troop;
}

echo json_encode($snapshot);