<?php
class Battle extends AppModel {

    var $name = 'Battle';
    var $actsAs = array('Containable', 'Linkable');

    //The Associations below have been created with all possible keys, those that are not needed can be removed
    var $belongsTo = array(
            'City' => array(
                            'className' => 'City',
                            'foreignKey' => 'city_id',
                            'conditions' => '',
                            'fields' => '',
                            'order' => ''
            )
    );

    var $hasMany = array(
            'BattleReport' => array(
                            'className' => 'BattleReport',
                            'foreignKey' => 'battle_id',
                            'dependent' => false,
                            'conditions' => 'BattleReport.ready = 1',
                            'fields' => '',
                            'order' => '',
                            'limit' => '',
                            'offset' => '',
                            'exclusive' => '',
                            'finderQuery' => '',
                            'counterQuery' => ''
            )
    );

    function listInvasionReports($cities, $returnOptions = false) {
        $options = array(
                'fields' => array(
                        'City.name',
                        'Battle.created',
                        'Battle.id',
                ),
                'conditions' => array(
                        'Battle.city_id' => $cities,
                        'NOT' => array('Battle.ended' => null)
                ),
                'link' => array(
                        'City' => array(
                                'fields' => array('name')
                        )
                ),
                'order' => 'Battle.ended DESC'
        );

        if ($returnOptions) return $options;

        return $this->find('all', $options);
    }

    function viewInvasionBattle($cities, $battleId) {
        $report = $this->find('first', array(
                'fields' => array(
                        'Battle.id',
                        'Battle.created'
                ),
                'conditions' => array(
                        'Battle.city_id' => $cities,
                        'Battle.id' => $battleId,
                        'NOT' => array('Battle.ended' => null)
                ),
                'link' => array()
        ));

        if (empty($report)) return false;

        return $report;
    }

    function viewInvasionReport($battleId, $returnOptions = false) {
        $options = array(
                'conditions' => array(
                        'BattleReport.battle_id' => $battleId
                ),
                'contain' => array(
                        'BattleReportTroop' => array(
                                'City' => array('fields' => array('id', 'name')),
                                'BattleReportObject' => array('order' => array('BattleReportObject.formation_type ASC', 'BattleReportObject.type ASC'))
                        )
                ),
                'order' => array(
                        'BattleReport.round ASC',
                        'BattleReport.turn ASC'
                )
        );

        if ($returnOptions) return $options;

        return $this->BattleReport->find('all', $options);
    }

    function listAttackReports($cities, $returnOptions = false) {
        $options = array(
                'joins' => array(
                        array(
                                'table' => 'battle_report_troops',
                                'alias' => 'BattleReportTroopEnter',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions'=> array(
                                        'BattleReportTroopEnter.battle_report_id = BattleReport.id',
                                        'BattleReportTroopEnter.state' => TROOP_STATE_ENTERING,
                                        'BattleReportTroopEnter.city_id' => $cities
                                )
                        ),
                        array(
                                'table' => 'battle_report_troops',
                                'alias' => 'BattleReportTroopExit',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions'=> array(
                                        'BattleReportTroopExit.group_id = BattleReportTroopEnter.group_id',
                                        'BattleReportTroopExit.state' => array(TROOP_STATE_RETREATING, TROOP_STATE_EXITING, TROOP_STATE_DYING)
                                )
                        ),
                        array(
                                'table' => 'battle_reports',
                                'alias' => 'BattleReportExit',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions'=> array(
                                        'BattleReportExit.id = BattleReportTroopExit.battle_report_id',
                                        'BattleReportExit.battle_id = BattleReport.battle_id',
                                        'BattleReportExit.ready = 1',
                                )
                        ),
                        array(
                                'table' => 'cities',
                                'alias' => 'TroopCity',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions'=> array(
                                        'BattleReportTroopEnter.city_id = TroopCity.id',
                                )
                        ),
                        array(
                                'table' => 'battles',
                                'alias' => 'Battle',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions' => array(
                                        'Battle.id = BattleReport.battle_id',
                                        'Battle.city_id NOT IN (' . implode(',', $cities) . ')', // Our own cities are covered by the invasion
                                )
                        ),
                        array(
                                'table' => 'cities',
                                'alias' => 'City',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions' => array(
                                        'City.id = Battle.city_id'
                                )
                        )
                ),
                'fields' => array(
                        'BattleReport.id',
                        'BattleReport.battle_id',
                        'BattleReport.round',
                        'BattleReport.turn',
                        'BattleReport.created',
                        'BattleReportTroopEnter.is_attacker',
                        'BattleReportTroopEnter.troop_stub_id',
                        'TroopCity.name',
                        'City.name'
                ),
                'conditions' => array(
                        'BattleReport.ready' => '1',
                )
        );

        if ($returnOptions) return $options;

        return $this->find('all', $options);

    }

    function viewAttackBattle($cities, $reportId) {
        $report = $this->BattleReport->find('first', array(
                'joins' => array(
                        array(
                                'table' => 'battle_report_troops',
                                'alias' => 'BattleReportTroopEnter',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions'=> array(
                                        'BattleReportTroopEnter.battle_report_id = BattleReport.id',
                                        'BattleReportTroopEnter.state' => TROOP_STATE_ENTERING,
                                        'BattleReportTroopEnter.city_id' => $cities
                                )
                        ),
                        array(
                                'table' => 'battle_report_troops',
                                'alias' => 'BattleReportTroopExit',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions'=> array(
                                        'BattleReportTroopExit.group_id = BattleReportTroopEnter.group_id',
                                        'BattleReportTroopExit.state' => array(TROOP_STATE_RETREATING, TROOP_STATE_EXITING, TROOP_STATE_DYING)
                                )
                        ),
                        array(
                                'table' => 'battle_reports',
                                'alias' => 'BattleReportExit',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions'=> array(
                                        'BattleReportExit.id = BattleReportTroopExit.battle_report_id',
                                        'BattleReportExit.battle_id = BattleReport.battle_id',
                                        'BattleReportExit.ready = 1',
                                )
                        ),
                        array(
                                'table' => 'cities',
                                'alias' => 'TroopCity',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions'=> array(
                                        'BattleReportTroopEnter.city_id = TroopCity.id',
                                )
                        ),
                        array(
                                'table' => 'battles',
                                'alias' => 'Battle',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions' => array(
                                        'Battle.id = BattleReport.battle_id',
                                        'Battle.city_id NOT IN (' . implode(',', $cities) . ')', // Our own cities are covered by the invasion
                                )
                        )
                ),
                'fields' => array(
                        'Battle.id',
                        'BattleReport.id',
                        'BattleReport.battle_id',
                        'BattleReport.round',
                        'BattleReport.turn',
                        'BattleReport.created',
                        'BattleReportExit.round',
                        'BattleReportExit.turn',
                        'BattleReportTroopEnter.is_attacker',
                        'BattleReportTroopEnter.troop_stub_id',
                        'TroopCity.name'
                ),
                'conditions' => array(
                        'BattleReport.ready' => '1',
                )
        ));

        if (empty($report)) return false;

        return $report;
    }

    function viewAttackReport($report, $returnOptions = false) {
        $options = array(
                'conditions' => array(
                        'BattleReport.battle_id' => $report['Battle']['id'],
                        'OR' => array(
                                array('BattleReport.round' => $report['BattleReport']['round'], 'BattleReport.turn >=' => $report['BattleReport']['turn']),
                                array('BattleReport.round >' => $report['BattleReport']['round'], 'BattleReport.round <' => $report['BattleReportExit']['round']),
                                array('BattleReport.round' => $report['BattleReportExit']['round'], 'BattleReport.turn <=' => $report['BattleReportExit']['turn'])
                        )
                ),
                'contain' => array(
                        'BattleReportTroop' => array(
                                'City' => array('fields' => array('id', 'name')),
                                'BattleReportObject' => array('order' => array('BattleReportObject.formation_type ASC', 'BattleReportObject.type ASC'))
                        )
                ),
                'order' => array(
                        'BattleReport.round ASC',
                        'BattleReport.turn ASC'
                )
        );

        if ($returnOptions) return $options;

        $this->BattleReport->find('all', $options);
    }

}