<?php

class BattleReportObject extends AppModel {

    var $name = 'BattleReportObject';
    var $belongsTo = array(
        'BattleReportTroop'
    );

}