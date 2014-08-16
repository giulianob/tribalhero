<?php

class TribeLog extends AppModel {

    var $name = 'TribeLog';
    
    var $belongsTo = array(
        'Tribe'
    );

    var $limitPerPage = 10;

    public function getListing($playerId, $page) {
        $Player = & ClassRegistry::init('Player');
        
        $tribeId = $Player->getTribeId($playerId);

        if (!$tribeId)
            return null;

        return array(
            'conditions' => array(
                'TribeLog.tribe_id' => $tribeId,
            ),
            'page' => $page,
            'limit' => $this->limitPerPage,
            'order' => array('TribeLog.created' => 'DESC'),
        );
    }

}
