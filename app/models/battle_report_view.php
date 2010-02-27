<?php
class BattleReportView extends AppModel {

    var $name = 'BattleReportView';
    var $actsAs = array('Containable', 'Linkable');
    var $order = array('BattleReportView.created DESC');

    var $belongsTo = array(
            'Battle' => array(
                            'className' => 'Battle',
                            'foreignKey' => 'battle_id',
                            'conditions' => '',
                            'fields' => '',
                            'order' => ''
            ),
            'BattleReportTroop' => array(
                            'className' => 'BattleReportTroop',
                            'foreignKey' => 'battle_report_troop_id',
                            'conditions' => '',
                            'fields' => '',
                            'order' => ''
            ),
            'City' => array(
                            'className' => 'City',
                            'foreignKey' => 'city_id',
                            'conditions' => '',
                            'fields' => '',
                            'order' => ''
            )
    );
}