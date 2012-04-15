<?php

class BattleReportTroop extends AppModel {

    var $name = 'BattleReportTroop';
    var $belongsTo = array(
        'BattleReport',
        'City'
    );
    var $hasMany = array(
        'BattleReportObject' => array('dependentBatchDelete' => true)
    );

}