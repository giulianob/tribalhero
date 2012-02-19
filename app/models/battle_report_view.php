<?php

class BattleReportView extends AppModel {

    var $name = 'BattleReportView';
    var $order = array('BattleReportView.created DESC');
    var $belongsTo = array(
        'Battle',
        'City',
        'BattleReportEnter' => array('className' => 'BattleReport'),
        'BattleReportExit' => array('className' => 'BattleReport'),
    );

}