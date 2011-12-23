<?php

class City extends AppModel {

    var $name = 'City';
    var $belongsTo = array(
        'Player',
    );
    var $hasMany = array(
        'BattleReportTroop',
        'Battle',
        'BattleReportView',
        'TroopStubList',
    );

}