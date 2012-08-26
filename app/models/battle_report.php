<?php

/**
 * @param BattleReportTroop $BattleReportTroop
 * @param Battle $Battle
 */
class BattleReport extends AppModel {

    var $name = 'BattleReport';
    var $order = array('BattleReport.created DESC');
    var $belongsTo = array(
        'Battle'
    );
    var $hasMany = array(
        'BattleReportTroop' => array('dependent' => true),
    );

    function paginateCount($conditions = array(), $recursive = 0, $extra = array()) {
        $parameters = compact('conditions');
        if ($recursive != $this->recursive) {
            $parameters['recursive'] = $recursive;
        }

        unset($extra['link']);
        return $this->find('count', array_merge($parameters, $extra));
    }

}