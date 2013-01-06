<?php

/**
 * @property Battle $Battle
 * @property BattleReportView $BattleReportView
 * @property Report $Report
 */
class UnreadReportsShell extends Shell {

    var $uses = array('Battle', 'BattleReportView', 'Report');

    /**
     * Finds all of the reports that ended in the past minute and sends all those users notifications with new report count
     */
    function main() {
        App::import('Core', 'Controller');
        App::import('Controller', 'AppController');
        App::import('Component', 'Thrift.Thrift');

        $this->Controller = & new AppController();        
        $this->Thrift = & new ThriftComponent(null);        
        $this->Thrift->initialize($this->Controller, $this->Controller->components['Thrift.Thrift']);

        // Finds all battles that ended within the past 65 secs (assuming cron job runs every 60 but give a few extra secs if time is a little off)
        $battles = $this->Battle->find('all', array(
            'conditions' => array(
                'Battle.ended >= DATE_SUB(UTC_TIMESTAMP(), INTERVAL 65 SECOND)',
                'Battle.owner_type' => 'City',
            ),
            'link' => array(
                    'City' => array(
                        'fields' => array('City.id'),
                        'Player' => array('fields' => array('Player.id')
                    ))
            ),
            'fields' => array('Battle.id'))
        );

        if (empty($battles)) {
            $this->out("Nothing to do");
            return;
        }

        // Contains all of the player ids we need to send out to (first we get the local ones then the remote ones after)
        $playerIds = Set::extract('/Player/id', $battles);

        // Find all battle report views from the battles above
        $battleIds = Set::extract('/Battle/id', $battles);
        $battleReportIds = $this->BattleReportView->find('all', array(
            'conditions' => array(
                'BattleReportView.battle_id' => $battleIds,
                'BattleReportView.owner_type' => 'City',
            ),
            'link' => array(
                    'City' => array(
                        'fields' => array('City.id'),
                        'Player' => array('fields' => array('Player.id')
                    )
            )))
        );

        // Get the unique ids from the local and remote reports
        $playerIds = array_unique(array_merge($playerIds, Set::extract('/Player/id', $battleReportIds)));

        // Foreach player, calculate their unread count
        $unreadCounts = array();
        foreach ($playerIds as $playerId) {
            $unreadCount = $this->Report->getUnreadCount($playerId);
            $unreadCounts[] = new PlayerUnreadCount(array('id' => $playerId, 'unreadCount' => $unreadCount));
        }

        if (empty($unreadCounts)) {
            $this->out("Nothing to do");
            return;
        }
        
        
        $this->out("Reporting for .. " . implode(',', $playerIds));

        try {
            $transport = $this->Thrift->getTransportFor('Notification');
            $protocol = $this->Thrift->getProtocol($transport);
            $notificationRpc = new NotificationClient($protocol);
            $transport->open();
            $notificationRpc->NewBattleReport($unreadCounts);
            $transport->close();
        }
        catch (Exception $e) {
            debug($e);
        }
    }

}
