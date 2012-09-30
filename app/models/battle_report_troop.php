<?php

class BattleReportTroop extends AppModel {

    var $name = 'BattleReportTroop';
    var $belongsTo = array(
        'BattleReport',
        'City' => array('conditions' => 'BattleReportTroop.owner_type = "City"', 'foreignKey' => 'owner_id', 'dependent' => false)
    );
    var $hasMany = array(
        'BattleReportObject' => array('dependentBatchDelete' => true)
    );

}