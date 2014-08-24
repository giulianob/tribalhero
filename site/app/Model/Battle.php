<?php

/**
 * @property City $City
 * @property BattleReport $BattleReport
 * @property BattleReportView $BattleReportView
 * @property BattleTribe $BattleTribe
 */
class Battle extends AppModel {

    var $name = 'Battle';
    var $belongsTo = array(
        'City' => array('conditions' => 'Battle.owner_type = "City"', 'foreignKey' => 'owner_id', 'dependent' => false)
    );
    var $hasMany = array(
        'BattleReport' => array('dependent' => true, 'conditions' => 'BattleReport.ready = 1'),
        'BattleReportView' => array('dependent' => true),
        'BattleTribe' => array('dependent' => true)
    );
    var $hasAndBelongsToMany = array(
        'Tribe' => array('with' => 'BattleTribe')
    );

    // Saves the names of locations for the current session
    var $locationCache = array();

    /**
     * Resolves the owner/location type and id to its name.
     * @param $type
     * @param $id
     * @return string
     */
    function getLocationName($type, $id) {

        if (isset($this->locationCache[$type . $id])) {
            return $this->locationCache[$type . $id];
        }

        $name = '';
        switch (strtolower($type)) {
            case 'city':
                /** @var $cityModel City */
                $cityModel = & ClassRegistry::init('City');
                $city = $cityModel->findById($id);
                if ($city) {
                    $name = $city['City']['name'];
                }
                break;
            case 'barbariantribe':
                return 'Barbarian Tribe';
            case 'stronghold':
            case 'strongholdgate':
                /** @var $strongholdModel Stronghold */
                $strongholdModel = & ClassRegistry::init('Stronghold');
                $stronghold = $strongholdModel->findById($id);
                if ($stronghold) {
                    $name = $stronghold['Stronghold']['name'];
                }
                break;
        }

        $this->locationCache[$type . $id] = $name;

        return $name;
    }

    /**
     * Returns the important battle events for the given battle
     */
    function viewBattleEvents($battle, $page) {
        $eventsPerPage = 3;

        if (!is_numeric($page)) {
            $page = 0;
        }

        $cacheKey = "battle_events_{$battle['Battle']['id']}_{$page}";

        Cache::set(array('duration' => '+10 days'));
        $cachedEvents = Cache::read($cacheKey);

        if ($cachedEvents !== false) {
            return $cachedEvents;
        }

        $reportsPages = ceil($this->BattleReport->find('count', array(
                    'conditions' => array('battle_id' => $battle['Battle']['id']))) / $eventsPerPage);

        // Find the battle reports we need to work with
        $reports = $this->BattleReport->find('all', array(
            'conditions' => array('battle_id' => $battle['Battle']['id']),
            'order' => array('id' => 'ASC'),
            'offset' => $page * $eventsPerPage,
            'limit' => $eventsPerPage));

        $outcome = array('pages' => $reportsPages, 'reports' => array());

        foreach ($reports as $report) {
            $reportOverview = array('id' => $report['BattleReport']['id'], 'round' => $report['BattleReport']['round'], 'turn' => $report['BattleReport']['turn']);

            // Find round total units and structures (we group by is_attacker and is_unit so we can actually get up to 4 rows returned which would be number of units/structures for attacker/defender
            $totalUnits = $this->BattleReport->BattleReportTroop->BattleReportObject->find('all', array(
                'group' => array('BattleReportTroop.is_attacker', 'is_unit'),
                'fields' => array('SUM(BattleReportObject.count) unit_count', 'BattleReportTroop.is_attacker', 'IF(BattleReportObject.type < 1000, true, false) is_unit'),
                'link' => array(
                    'BattleReportTroop' => array(
                        'type' => 'INNER',
                        'fields' => array(),
                        'conditions' => array('BattleReportTroop.battle_report_id' => $report['BattleReport']['id']),
                ))));

            $reportOverview['attackerUnits'] = 0;
            $reportOverview['attackerStructures'] = 0;
            $reportOverview['defenderUnits'] = 0;
            $reportOverview['defenderStructures'] = 0;
            foreach ($totalUnits as $totalUnit) {
                if ($totalUnit['BattleReportTroop']['is_attacker'] && $totalUnit[0]['is_unit']) {
                    $reportOverview['attackerUnits'] = $totalUnit[0]['unit_count'];
                }
                else if (!$totalUnit['BattleReportTroop']['is_attacker'] && $totalUnit[0]['is_unit']) {
                    $reportOverview['defenderUnits'] = $totalUnit[0]['unit_count'];
                }
                else if ($totalUnit['BattleReportTroop']['is_attacker'] && !$totalUnit[0]['is_unit']) {
                    $reportOverview['attackerStructures'] = $totalUnit[0]['unit_count'];
                }
                else if (!$totalUnit['BattleReportTroop']['is_attacker'] && !$totalUnit[0]['is_unit']) {
                    $reportOverview['defenderStructures'] = $totalUnit[0]['unit_count'];
                }
            }

            // Find total troops
            $totalTroops = $this->BattleReport->BattleReportTroop->find('all', array(
                'group' => 'BattleReportTroop.is_attacker',
                'fields' => array('COUNT(BattleReportTroop.id) troop_count', 'is_attacker'),
                'conditions' => array('BattleReportTroop.battle_report_id' => $report['BattleReport']['id'])));

            $reportOverview['attackerTroops'] = 0;
            $reportOverview['defenderTroops'] = 0;
            foreach ($totalTroops as $totalTroop) {
                if ($totalTroop['BattleReportTroop']['is_attacker']) {
                    $reportOverview['attackerTroops'] = $totalTroop[0]['troop_count'];
                }
                else {
                    $reportOverview['defenderTroops'] = $totalTroop[0]['troop_count'];
                }
            }

            // Find important battle events
            $importantTroops = $this->BattleReport->BattleReportTroop->find('all', array(
                'fields' => array('BattleReportTroop.id', 'BattleReportTroop.state', 'BattleReportTroop.group_id', 'BattleReportTroop.is_attacker', 'BattleReportTroop.owner_id', 'BattleReportTroop.owner_type', 'BattleReportTroop.name'),
                'order' => array('BattleReportTroop.group_id' => 'ASC'),
                'link' => array(
                ),
                'conditions' => array('BattleReportTroop.battle_report_id' => $report['BattleReport']['id'], 'NOT' => array('BattleReportTroop.state' => TROOP_STATE_STAYING))));

            $unitTotals = $this->BattleReport->BattleReportTroop->BattleReportObject->find('all', array(
                'fields' => array('SUM(BattleReportObject.count) unit_count', 'BattleReportObject.type', 'BattleReportObject.battle_report_troop_id'),
                'conditions' => array(
                    'BattleReportObject.battle_report_troop_id' => Set::extract('/BattleReportTroop/id', $importantTroops)
                ),
                'order' => array('BattleReportObject.type' => 'DESC'),
                'group' => array('BattleReportObject.battle_report_troop_id', 'BattleReportObject.type')));

            $unitTotalsByTroop = array();
            foreach ($importantTroops as $importantTroop) {
                $unitTotalsByTroop[$importantTroop['BattleReportTroop']['id']] = array(
                    'id' => $importantTroop['BattleReportTroop']['id'],
                    'state' => $importantTroop['BattleReportTroop']['state'],
                    'groupId' => $importantTroop['BattleReportTroop']['group_id'],
                    'isAttacker' => $importantTroop['BattleReportTroop']['is_attacker'] == true,
                    'name' => $importantTroop['BattleReportTroop']['name'],
                    'owner' => array(
                        'id' => $importantTroop['BattleReportTroop']['owner_id'],
                        'type' => $importantTroop['BattleReportTroop']['owner_type'],
                        'name' => $this->getLocationName($importantTroop['BattleReportTroop']['owner_type'], $importantTroop['BattleReportTroop']['owner_id'])
                    ),
                    'units' => array(),
                );
            }

            foreach ($unitTotals as $unitTotal) {
                $troopId = $unitTotal['BattleReportObject']['battle_report_troop_id'];
                $unitType = $unitTotal['BattleReportObject']['type'];

                $unitTotalsByTroop[$troopId]['units'][] = array(
                    'type' => $unitType,
                    'count' => $unitTotal[0]['unit_count']
                );
            }

            $reportOverview['events'] = array_values($unitTotalsByTroop);

            $outcome['reports'][] = $reportOverview;
        }

        Cache::set(array('duration' => '+10 days'));
        Cache::write($cacheKey, $outcome);

        return $outcome;
    }

    /**
     * Returns the results for a group for a given battle
     * @param type $battleId
     */
    function viewGroupOutcome($battle, $groupId) {
        App::uses('Sanitize', 'Utility');

        $battleId = $battle['Battle']['id'];

        // Get the count for each object at the time that they joined
        $groupJoinCount = $this->getBattleReportObjectJoinOrLeaveInfo($battleId, $groupId, 'JOIN', array('BattleReportObject.object_id', 'BattleReportObject.count'));
        $groupJoinCount = Set::combine($groupJoinCount, '/BattleReportObject/object_id', '/BattleReportObject/count');

        // Get the info for each object at the time that they left the battle
        $groupLeaveInfo = $this->getBattleReportObjectJoinOrLeaveInfo($battleId, $groupId, 'LEAVE', array(
            'BattleReportObject.type',
            'BattleReportObject.object_id',
            'BattleReportObject.level',
            'BattleReportObject.count',
            'BattleReportObject.damage_taken',
            'BattleReportObject.damage_dealt',
            'BattleReportObject.hits_dealt',
            'BattleReportObject.hits_received',
            'BattleReportObject.hp'));

        $result = array();
        foreach ($groupLeaveInfo as $group) {
            $objectId = $group['BattleReportObject']['object_id'];
            $leaveInfo = $group['BattleReportObject'];
            $result[] = array(
                'type' => $leaveInfo['type'],
                'level' => $leaveInfo['level'],
                'objectId' => $leaveInfo['object_id'],
                'count' => $leaveInfo['count'],
                'joinCount' => $groupJoinCount[$objectId],
                'damageTaken' => $leaveInfo['damage_taken'],
                'damageDealt' => $leaveInfo['damage_dealt'],
                'hitsDealt' => $leaveInfo['hits_dealt'],
                'hitsTaken' => $leaveInfo['hits_received'],
                'hp' => $leaveInfo['hp'],
            );
        }

        return $result;
    }

    /**
     * Returns information for troops at the time they entered/left the battle. The fields and conditions returned can be modified.
     * @param type $battleId
     * @param type $groupId
     * @param type $joinOrLeave
     * @param type $fields
     * @param type $conditions
     * @return type
     */
    private function getBattleReportObjectJoinOrLeaveInfo($battleId, $groupId, $joinOrLeave, $fields, $conditions = array()) {
        App::uses('Sanitize', 'Utility');

        if ($groupId) {
            $conditions["BattleReportTroopJoin.group_id"] = $groupId;
        }

        $dbo = $this->getDataSource();

        // The subquery will return the troop id at either the time they joined/left the battle. This is then used to join to the actual records.
        return $this->query(sprintf("
          SELECT %s
          FROM `battle_report_objects` AS `BattleReportObject`
          INNER JOIN `battle_report_troops` AS `BattleReportTroop` ON (`BattleReportTroop`.`id` = `BattleReportObject`.`battle_report_troop_id`),
            (
                SELECT %s(`BattleReportObjectJoin`.`id`) `id`
                FROM `battle_report_objects` AS `BattleReportObjectJoin`
                INNER JOIN `battle_report_troops` AS `BattleReportTroopJoin` ON (`BattleReportTroopJoin`.`id` = `BattleReportObjectJoin`.`battle_report_troop_id`)
                INNER JOIN `battle_reports` AS `BattleReportJoin` ON (`BattleReportJoin`.`battle_id` = %s AND `BattleReportJoin`.`id` = `BattleReportTroopJoin`.`battle_report_id`)
                %s
                GROUP BY `BattleReportObjectJoin`.`object_id`
            ) as `HighestObject`
          WHERE `BattleReportObject`.`id` = HighestObject.id
          ORDER BY `BattleReportObject`.`type` DESC, `BattleReportObject`.`id` ASC", implode(',', $dbo->fields($this->BattleReport->BattleReportTroop->BattleReportObject, null, $fields)), $joinOrLeave == 'JOIN' ? 'MIN' : 'MAX', Sanitize::escape($battleId), $dbo->conditions($conditions)));
    }

    /**
     * Returns the battle outcome overview for the specified $battle
     * @param type $battle
     * @return type
     */
    function viewBattleOutcome($battle) {
        $cacheKey = "battle_outcome_{$battle['Battle']['id']}";

        Cache::set(array('duration' => '+10 days'));
        $cachedOutcome = Cache::read($cacheKey);

        if ($cachedOutcome !== false) {
            return $cachedOutcome;
        }

        $battleId = $battle['Battle']['id'];

        $outcome = array();

        // Get total battle time
        $outcome['timeLasted'] = strtotime($battle['Battle']['ended']) - strtotime($battle['Battle']['created']);

        // Get total structures destroyed
        $outcome['destroyedStructures'] = $this->BattleReport->BattleReportTroop->BattleReportObject->find('count', array(
            'conditions' => array(
                'BattleReportObject.type >' => 1000, // Structures only
                'BattleReportObject.count' => 0
            ),
            'link' => array(
                'BattleReportTroop' => array(
                    'conditions' => array('BattleReportTroop.group_id' => 1), // Local troop only
                    'type' => 'INNER',
                    'fields' => array(),
                    'BattleReport' => array(
                        'type' => 'INNER',
                        'conditions' => array('BattleReport.battle_id' => $battleId),
                        'fields' => array()
                    )
            ))));

        // Find total attackers and defenders at time they joined the battle
        $joinCount = $this->getBattleReportObjectJoinOrLeaveInfo($battleId, null, 'JOIN', array('BattleReportObject.count', 'BattleReportTroop.is_attacker'), array('BattleReportObjectJoin.type <' => 1000));
        $outcome['attackerJoinCount'] = 0;
        $outcome['defenderJoinCount'] = 0;
        foreach ($joinCount as $count) {
            if ($count['BattleReportTroop']['is_attacker']) {
                $outcome['attackerJoinCount'] += $count['BattleReportObject']['count'];
            }
            else {
                $outcome['defenderJoinCount'] += $count['BattleReportObject']['count'];
            }
        }

        // Find total count of attackers and defenders at time they left the battle
        $leaveCount = $this->getBattleReportObjectJoinOrLeaveInfo($battleId, null, 'LEAVE', array('BattleReportObject.count', 'BattleReportTroop.is_attacker'), array('BattleReportObjectJoin.type <' => 1000));
        $outcome['attackerLeaveCount'] = 0;
        $outcome['defenderLeaveCount'] = 0;
        foreach ($leaveCount as $count) {
            if ($count['BattleReportTroop']['is_attacker']) {
                $outcome['attackerLeaveCount'] += $count['BattleReportObject']['count'];
            }
            else {
                $outcome['defenderLeaveCount'] += $count['BattleReportObject']['count'];
            }
        }

        // Find total loot stolen
        $totalLoot = $this->BattleReport->BattleReportTroop->find('all', array(
            'fields' => array('SUM(BattleReportTroop.wood) totalWoodLooted', 'SUM(BattleReportTroop.iron) totalIronLooted', 'SUM(BattleReportTroop.crop) totalCropLooted', 'SUM(BattleReportTroop.gold) totalGoldLooted'),
            'conditions' => array(
                'BattleReportTroop.is_attacker' => 1,
                'BattleReportTroop.state' => array(TROOP_STATE_EXITING, TROOP_STATE_OUT_OF_STAMINA, TROOP_STATE_RETREATING)
            ),
            'link' => array('BattleReport' => array(
                    'type' => 'INNER',
                    'conditions' => array('BattleReport.battle_id' => $battleId)
            ))));

        $outcome = array_merge($outcome, reset(reset($totalLoot)));

        // Find tribes that participated in the battle
        $tribesParticipated = $this->BattleTribe->find('all', array(
            'fields' => array('BattleTribe.is_attacker'),
            'conditions' => array('BattleTribe.battle_id' => $battleId),
            'link' => array(
                'Tribe' => array(
                    'type' => 'INNER',
                    'fields' => array('Tribe.id', 'Tribe.name')
                )
            )
        ));

        $outcome['attackerTribes'] = array();
        $outcome['defenderTribes'] = array();
        foreach ($tribesParticipated as $tribeParticipated) {
            if ($tribeParticipated['BattleTribe']['is_attacker']) {
                $outcome['attackerTribes'][] = array('id' => $tribeParticipated['Tribe']['id'], 'name' => $tribeParticipated['Tribe']['name']);
            }
            else {
                $outcome['defenderTribes'][] = array('id' => $tribeParticipated['Tribe']['id'], 'name' => $tribeParticipated['Tribe']['name']);
            }
        }

        Cache::set(array('duration' => '+10 days'));
        Cache::write($cacheKey, $outcome);

        return $outcome;
    }

    /**
     * Lists all of the invasion reports for a list of places.
     * Optionally further filter based on a location
     */
    function listInvasionReports($viewType, $ownerType, $ownerIds, $locationType = null, $locationId = null) {
        $options = array(
            'fields' => array(
                'Battle.created',
                'Battle.id',
                'Battle.read',
                'Battle.location_type',
                'Battle.location_id',
            ),
            'conditions' => array(
                'NOT' => array('Battle.ended' => null)
            ),
            'contain' => array(
                'Tribe' => array(
                    'fields' => array('Tribe.id', 'Tribe.name')
                )
            ),
            'order' => array('Battle.ended' => 'DESC')
        );

        $options['conditions']['Battle.owner_type'] = $ownerType;

        if ($ownerIds !== "*") {
            $options['conditions']['Battle.owner_id'] = $ownerIds;
        }

        if ($locationType !== false && $locationId !== false) {
            $options['conditions']['Battle.location_type'] = $locationType;
            $options['conditions']['Battle.location_id'] = $locationId;
        }

        return $options;
    }

    function viewInvasionReport($ownerType, $ownerId, $battleId, &$outcomeOnly, &$groupId, &$isUnread, &$loot) {
        $loot = null;
        $outcomeOnly = false;
        $groupId = $ownerType == 'City' ? 1 : 0;
        $isUnread = false;

        $options = array(
            'fields' => array(
                'Battle.id',
                'Battle.created',
                'Battle.ended',
                'Battle.read'
            ),
            'conditions' => array(
                'Battle.id' => $battleId,
                'NOT' => array('Battle.ended' => null)
            ),
            'link' => array()
        );

        $options['conditions']['Battle.owner_type'] = $ownerType;

        if ($ownerId !== "*") {
            $options['conditions']['Battle.owner_id'] = $ownerId;
        }

        $report = $this->find('first', $options);

        if (empty($report))
            return false;

        $isUnread = $report['Battle']['read'] == 0;

        return $report;
    }

    /**
     * Returns a single battle report (snapshot)
     * @param int $reportId
     * @return array
     */
    function viewSnapshot($battleId, $reportId) {
        // We could just accept the reportId here but then it would take an extra lookup to make sure the user is not trying to see a snapshot for
        // the wrong battle so we just take in both to save 1 query.
        $options = array(
            'conditions' => array(
                'BattleReport.id' => $reportId,
                'BattleReport.battle_id' => $battleId,
            ),
            'contain' => array(
                'BattleReportTroop' => array(
                    'order' => array('BattleReportTroop.group_id' => 'ASC'),
                    'BattleReportObject' => array('order' => array('BattleReportObject.type' => 'ASC', 'BattleReportObject.object_id' => 'ASC'))
                )
            )
        );

        $battleReport = $this->BattleReport->find('first', $options);

        if (empty($battleReport)) {
            return false;
        }

        foreach ($battleReport['BattleReportTroop'] as $k => $troop) {
            $battleReport['BattleReportTroop'][$k]['owner'] = array(
                'id' => $troop['owner_id'],
                'type' => $troop['owner_type'],
                'name' => $this->getLocationName($troop['owner_type'], $troop['owner_id'])
            );
            unset($battleReport['BattleReportTroop'][$k]['owner_id']);
            unset($battleReport['BattleReportTroop'][$k]['owner_type']);
        }

        return $battleReport;
    }

    /**
     * Lists all of the attack reports for a list of cities
     * @return array List of reports
     */
    function listAttackReports($viewType, $ownerType, $ownerId, $locationType, $locationId) {
        $options = array(
            'conditions' => array('Battle.ended IS NOT NULL'),
            'contain' => array(
                'Tribe' => array(
                    'fields' => array('Tribe.id', 'Tribe.name')
                )
            ),
            'link' => array(
                'BattleReportView' => array('type' => 'INNER')
            ),
            'fields' => array(
                'BattleReportView.id',
                'BattleReportView.battle_id',
                'BattleReportView.created',
                'BattleReportView.owner_id',
                'BattleReportView.owner_type',
                'BattleReportView.is_attacker',
                'BattleReportView.troop_stub_id',
                'BattleReportView.read',
                'Battle.ended',
                'Battle.location_type',
                'Battle.location_id',
            ),
            'order' => array('Battle.ended' => 'DESC')
        );

        $options['conditions']['BattleReportView.owner_type'] = $ownerType;

        if ($ownerId !== "*") {
            $options['conditions']['BattleReportView.owner_id'] = $ownerId;
        }

        if ($locationType !== false && $locationId !== false) {
            $options['conditions']['Battle.location_type'] = $locationType;
            $options['conditions']['Battle.location_id'] = $locationId;
        }

        return $options;
    }

    /**
     * Returns the specified attack report for a list of cities
     * @param array $cities
     * @param int $reportViewId
     * @return array
     */
    function viewAttackReport($ownerType, $ownerId, $reportViewId, &$outcomeOnly, &$groupId, &$isUnread, &$loot) {
        $outcomeOnly = false;

        $options = array(
            'link' => array(
                'Battle' => array('conditions' => array('NOT' => array('Battle.ended' => null))),
            ),
            'fields' => array(
                'Battle.id',
                'Battle.owner_type',
                'Battle.owner_id',
                'BattleReportView.id',
                'BattleReportView.battle_id',
                'BattleReportView.is_attacker',
                'BattleReportView.group_id',
                'BattleReportView.read',
                'BattleReportView.loot_crop',
                'BattleReportView.loot_wood',
                'BattleReportView.loot_iron',
                'BattleReportView.loot_gold',
                'BattleReportView.bonus_crop',
                'BattleReportView.bonus_wood',
                'BattleReportView.bonus_iron',
                'BattleReportView.bonus_gold',
            ),
            'conditions' => array(
                'BattleReportView.id' => $reportViewId,
            )
        );

        $options['conditions']['BattleReportView.owner_type'] = $ownerType;

        if ($ownerId !== "*") {
            $options['conditions']['BattleReportView.owner_id'] = $ownerId;
        }

        $report = $this->BattleReportView->find('first', $options);

        if (empty($report))
            return false;

        $groupId = $report['BattleReportView']['group_id'];
        $isUnread = $report['BattleReportView']['read'] == false;
        $loot = array(
            'crop' => $report['BattleReportView']['loot_crop'],
            'gold' => $report['BattleReportView']['loot_gold'],
            'iron' => $report['BattleReportView']['loot_iron'],
            'wood' => $report['BattleReportView']['loot_wood'],
            'bonus' => array(
                'crop' => $report['BattleReportView']['bonus_crop'],
                'gold' => $report['BattleReportView']['bonus_gold'],
                'iron' => $report['BattleReportView']['bonus_iron'],
                'wood' => $report['BattleReportView']['bonus_wood'],
            ),
        );

        $joinAndLeaveReportIds = reset(reset($this->BattleReport->BattleReportTroop->find('all', array(
            'link' => array(
                'BattleReport' => array(
                    'type' => 'INNER',
                    'conditions' => array('BattleReport.battle_id' => $report['BattleReportView']['battle_id']),
                    'fields' => array()
                )
            ),
            'conditions' => array(
                'BattleReportTroop.group_id' => $groupId
            ),
            'fields' => array(
                'MIN(BattleReportTroop.battle_report_id) as join_battle_report_id',
                'MAX(BattleReportTroop.battle_report_id) as leave_battle_report_id',
                'MIN(BattleReportTroop.id) as join_battle_report_troop_id',
                'MAX(BattleReportTroop.id) as leave_battle_report_troop_id'
        )))));

        $report['BattleReportEnter'] = reset($this->BattleReport->findById($joinAndLeaveReportIds['join_battle_report_id']));
        $report['BattleReportExit'] = reset($this->BattleReport->findById($joinAndLeaveReportIds['leave_battle_report_id']));
        // For city battles and barb tribes, the min round rule applies
        if ($report['Battle']['location_type'] == 'City' || $report['Battle']['location_type'] == 'BarbarianTribe') {
            $roundDelta = intVal($report['BattleReportExit']['round']) - intVal($report['BattleReportEnter']['round']);
            if ($roundDelta < BATTLE_VIEW_MIN_ROUND) {
                // Get exit troop
                $exitTroop = $this->BattleReport->BattleReportTroop->findById($joinAndLeaveReportIds['leave_battle_report_troop_id']);
                // If troop left and they were either defenders OR they weren't exiting or out of stamina then they cant see
                if (!$report['BattleReportView']['is_attacker'] ||  !in_array($exitTroop['BattleReportTroop']['state'], array(TROOP_STATE_EXITING, TROOP_STATE_OUT_OF_STAMINA))) {
                    $outcomeOnly = true;
                }
            }
        }

        unset($report['BattleReportView']);

        return $report;
    }

}