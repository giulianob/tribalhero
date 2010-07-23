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
                            'dependent' => true,
                            'conditions' => 'BattleReport.ready = 1',
                            'fields' => '',
                            'order' => '',
                            'limit' => '',
                            'offset' => '',
                            'exclusive' => '',
                            'finderQuery' => '',
                            'counterQuery' => ''
            ),
            'BattleReportView' => array(
                            'className' => 'BattleReportView',
                            'foreignKey' => 'battle_id',
                            'dependent' => true,
                            'conditions' => '',
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

    function viewInvasionReport($cities, $battleId) {
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

    function viewBattle($battleId, $returnOptions = false) {
        $options = array(
                'conditions' => array(
                        'BattleReport.battle_id' => $battleId
                ),
                'contain' => array(
                        'BattleReportTroop' => array(
                                'order' => array('BattleReportTroop.group_id ASC'),
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
                                'table' => 'battles',
                                'alias' => 'Battle',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions'=> array(
                                        'BattleReportView.battle_id = Battle.id',
                                        'Battle.ended IS NOT NULL',
                                )
                        ),
                        array(
                                'table' => 'cities',
                                'alias' => 'City',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions'=> array(
                                        'Battle.city_id = City.id'
                                )
                        ),
                        array(
                                'table' => 'cities',
                                'alias' => 'TroopCity',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions'=> array(
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
                        'TroopCity.name',
                        'City.name'
                ),
                'conditions' => array(
                        'BattleReportView.city_id' => $cities
                )
        );

        if ($returnOptions) return $options;

        return $this->BattleReportView->find('all', $options);
    }


    function viewAttackReport($cities, $reportViewId) {
        $report = $this->BattleReportView->find('first', array(
                'joins' => array(
                        array(
                                'table' => 'battles',
                                'alias' => 'Battle',
                                'type' => 'INNER',
                                'foreignKey' => false,
                                'conditions'=> array(
                                        'BattleReportView.battle_id = Battle.id',
                                        'Battle.ended IS NOT NULL',
                                )
                        )
                ),
                'fields' => array(
                        'BattleReportView.id',
                        'BattleReportView.battle_id',
                        'BattleReportView.is_attacker',
                        'BattleReportView.group_id'
                ),
                'conditions' => array(
                        'BattleReportView.id' => $reportViewId,
                        'BattleReportView.city_id' => $cities,
                )

        ));
        
        if (empty($report)) return false;

        return $report;
    }

}