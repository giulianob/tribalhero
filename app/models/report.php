<?php

class Report extends AppModel {

    var $name = 'Report';
    var $useTable = false;

    function getUnreadCount($playerId) {
        $Battle = & ClassRegistry::init('Battle');

        $localUnread = $Battle->find('count', array(
                    'link' => array(
                        'City' => array(
                            'type' => 'inner'
                        ),
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
                        'City' => array('type' => 'inner'),
                        'Battle' => array('type' => 'inner')
                    ),
                    'conditions' => array(
                        'City.player_id' => $playerId,
                        'BattleReportView.read' => false,
                        'NOT' => array(
                            'Battle.ended' => null
                        )
                    )
                ));

        return $localUnread + $remoteUnread;
    }
	
    function markAllAsRead($playerId) {

        $City = & ClassRegistry::init('City');
        $Battle = & ClassRegistry::init('Battle');

        // Get list of cities for given player
        $cities = $City->find('all', array(
                    'contain' => array(),
                    'conditions' => array(
                        'player_id' => $playerId
                    )
                ));

        $cityIds = Set::extract('{n}.City.id', $cities);

		// Local
		$Battle->updateAll(array(
			'Battle.read' => true
				),
				array(
                    'Battle.owner_type' => 'City',
					'Battle.owner_id' => $cityIds,
					'NOT' => array(
						'Battle.ended' => null
				))
		);

		// Remote
		$Battle->BattleReportView->updateAll(array(
			'BattleReportView.read' => true
				),
				array(
                    'BattleReportView.owner_type' => 'City',
                    'BattleReportView.owner_id' => $cityIds,
				)
		);        
    }
	
    function markAsRead($playerId, $local, $id) {

        $City = & ClassRegistry::init('City');
        $Battle = & ClassRegistry::init('Battle');

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
                        'Battle.owner_type' => 'City',
                        'Battle.owner_id' => $cityIds,
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
                        'BattleReportView.owner_type' => 'City',
                        'BattleReportView.owner_id' => $cityIds,
                    )
            );
        }
    }

}