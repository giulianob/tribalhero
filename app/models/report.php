<?php

class Report extends AppModel {
    var $useTable = false;

    function getUnreadCount($playerId) {
        $Battle =& ClassRegistry::init('Battle');

        $localUnread = $Battle->find('count', array(
                'link' => array(
                        'City' => array('type' => 'inner'),
                ),
                'conditions' => array(
                        'City.player_id' => $playerId,
                        'NOT' => array(
                            'Battle.ended' => null
                        ),
                        'Battle.read' => false
                )
        ));

        $remoteUnread = $Battle->BattleReportView->find('count', array(
                'link' => array(
                        'City' => array('type' => 'inner')
                ),
                'conditions' => array(
                        'City.player_id' => $playerId,
                        'BattleReportView.read' => false
                )
        ));

        return $localUnread + $remoteUnread;
    }

    function markAsRead($playerId, $local, $id) {

        $City =& ClassRegistry::init('City');
        $Battle =& ClassRegistry::init('Battle');

        // Get list of cities for given player
        $cities = $City->find('all', array(
                'contain' => array(),
                'conditions' => array(
                        'player_id' => $playerId
                )
        ));

        $cityIds = Set::extract('{n}.City.id', $cities);

        if ($local) {
            $Battle->updateAll(array(
                    'Battle.read' => true
                    ),
                    array(
                    'Battle.id' => $id,
                    'Battle.city_id' => $cityIds,
                    'NOT' => array(
                            'Battle.ended' => null
                    ))
            );
        } else {
            $Battle->BattleReportView->updateAll(array(
                    'BattleReportView.read' => true
                    ),
                    array(
                    'BattleReportView.id' => $id,
                    'BattleReportView.city_id' => $cityIds,
                    )
            );
        }
    }
}