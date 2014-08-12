<?php

class BattleReportView extends AppModel {

    var $order = array('BattleReportView.created DESC');

    var $belongsTo = array(
        'Battle',
        'City' => array('conditions' => 'BattleReportView.owner_type = "City"', 'foreignKey' => 'owner_id', 'dependent' => false)
    );

}