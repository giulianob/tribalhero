<?php

/**
 * @property City $City
 */
class Battle extends AppModel {

    var $name = 'Battle';
    var $belongsTo = array(
        'City'
    );
    var $hasMany = array(
        'BattleReport' => array('dependent' => true, 'conditions' => 'BattleReport.ready = 1'),
        'BattleReportView' => array('dependent' => true)
    );

    /**
     * Lists all of the invasion reports for a list of cities
     * @param array $cities List of cities to find reports on
     * @param bool $returnOptions Whether to return the conditions or perform the find
     * @return array List of reports
     */
    function listInvasionReports($cities, $returnOptions = false) {
        $options = array(
            'fields' => array(
                'City.name',
                'Battle.created',
                'Battle.id',
                'Battle.read'
            ),
            'conditions' => array(
                'NOT' => array('Battle.ended' => null)
            ),
            'link' => array(
                'City' => array(
                    'fields' => array('name')
                )
            ),
            'order' => 'Battle.ended DESC'
        );

        if (!is_null($cities)) {
            $options['conditions']['Battle.city_id'] = $cities;
        }

        if ($returnOptions)
            return $options;

        return $this->find('all', $options);
    }

    /**
     * Returns a given report for a list of cities and given battle id
     * @param array $cities List of cities to search on. If null, searches all cities.
     * @param int $battleId
     * @return array Report found
     */
    function viewInvasionReport($cities, $battleId) {
        $options = array(
            'fields' => array(
                'Battle.id',
                'Battle.created',
                'Battle.read'
            ),
            'conditions' => array(
                'Battle.id' => $battleId,
                'NOT' => array('Battle.ended' => null)
            ),
            'link' => array()
        );

        if (!is_null($cities)) {
            $options['conditions']['Battle.city_id'] = $cities;
        }

        $report = $this->find('first', $options);

        if (empty($report))
            return false;

        return $report;
    }

    /**
     * Returns the battle report for a given battle
     * @param int $battleId
     * @param bool $returnOptions Whether to return the conditions or perform the find
     * @return array
     */
    function viewBattle($battleId, $returnOptions = false) {
        $options = array(
            'conditions' => array(
                'BattleReport.battle_id' => $battleId
            ),
            'contain' => array(
                'BattleReportTroop' => array(
                    'order' => array('BattleReportTroop.group_id ASC'),
                    'City' => array('fields' => array('id', 'name'),
                        'Player' => array('fields' => array('id', 'name')),
                    ),
                    'BattleReportObject' => array('order' => array('BattleReportObject.type ASC', 'BattleReportObject.object_id ASC'))
                )
            ),
            'order' => array(
                'BattleReport.round ASC',
                'BattleReport.turn ASC'
            )
        );

        if ($returnOptions)
            return $options;

        return $this->BattleReport->find('all', $options);
    }

    /**
     * Lists all of the attack reports for a list of cities
     * @param array $cities List of cities to find reports on
     * @param bool $returnOptions Whether to return the conditions or perform the find
     * @return array List of reports
     */
    function listAttackReports($cities, $returnOptions = false) {
        $options = array(
            'joins' => array(
                array(
                    'table' => 'battles',
                    'alias' => 'Battle',
                    'type' => 'INNER',
                    'foreignKey' => false,
                    'conditions' => array(
                        'BattleReportView.battle_id = Battle.id',
                        'Battle.ended IS NOT NULL',
                    )
                ),
                array(
                    'table' => 'cities',
                    'alias' => 'City',
                    'type' => 'INNER',
                    'foreignKey' => false,
                    'conditions' => array(
                        'Battle.city_id = City.id'
                    )
                ),
                array(
                    'table' => 'cities',
                    'alias' => 'TroopCity',
                    'type' => 'INNER',
                    'foreignKey' => false,
                    'conditions' => array(
                        'BattleReportView.city_id = TroopCity.id'
                    )
                )
            ),
            'fields' => array(
                'BattleReportView.id',
                'BattleReportView.battle_id',
                'BattleReportView.created',
                'BattleReportView.is_attacker',
                'BattleReportView.troop_stub_id',
                'BattleReportView.read',
                'TroopCity.name',
                'City.name'
            ),
            'conditions' => array(
            )
        );

        if (!is_null($cities)) {
            $options['conditions']['BattleReportView.city_id'] = $cities;
        }

        if ($returnOptions)
            return $options;

        return $this->BattleReportView->find('all', $options);
    }

    /**
     * Returns the specified attack report for a list of cities
     * @param array $cities
     * @param int $reportViewId
     * @return array
     */
    function viewAttackReport($cities, $reportViewId) {
        $options = array(
            'joins' => array(
                array(
                    'table' => 'battles',
                    'alias' => 'Battle',
                    'type' => 'INNER',
                    'foreignKey' => false,
                    'conditions' => array(
                        'BattleReportView.battle_id = Battle.id',
                        'Battle.ended IS NOT NULL',
                    )
                )
            ),
            'fields' => array(
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

        if (!is_null($cities)) {
            $options['conditions']['BattleReportView.city_id'] = $cities;
        }

        $report = $this->BattleReportView->find('first', $options);

        if (empty($report))
            return false;

        return $report;
    }

}