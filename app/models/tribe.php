<?php

class Tribe extends AppModel {

    var $name = 'Tribe';

    var $belongsTo = array(
        'Player',
    );
    var $hasMany = array(
        'Tribesman',
    );

    function findTribesman($playerId) {
        return $this->Tribesman->find('first', array(
             'conditions' => array('Tribesman.player_id' => $playerId),
             'link' => array('Tribe')
        ));          
    }

    function hasRight($right, $tribesman) {
        $ranks = json_decode($tribesman['Tribe']['ranks'], true);
        $permission = $ranks[$tribesman['Tribesman']['rank']]['Permission'];

        if ($permission & 0x01) return true;
              
        switch($right) {
            case 'post_delete': 
                return $permission & 0x80;
        }
        return false;
    }
}