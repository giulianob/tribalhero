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
        // Finds all battles that ended within the past 65 secs (assuming cron job runs every 60 but give a

        // We need to use thrift so we're manually initializing a controller here
        // Ideally the thrift logic wouldnt be in a component that cant be instantiated easily elsewhere
        App::uses('CakeRequest', 'Network');
        App::uses('CakeResponse', 'Network');
        App::uses('Controller', 'Controller');
        App::uses('AppController', 'Controller');
        $controller = new AppController(new CakeRequest(), new CakeResponse());
        $controller->constructClasses();
        $controller->startupProcess();

        // Finds all battles that ended within the past 65 secs (assuming cron job runs every 60 but give a few extra secs if time is a little off)
        $localBattles = $this->Battle->find('all', array(
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

        // Find all remote battles
        $battleReportView = $this->BattleReportView->find('all', array(
            'conditions' => array(
                'Battle.ended >= DATE_SUB(UTC_TIMESTAMP(), INTERVAL 65 SECOND)',
                'BattleReportView.owner_type' => 'City',
            ),
            'link' => array(
                    'City' => array(
                        'fields' => array('City.id'),
                        'Player' => array('fields' => array('Player.id'))
                    ),
                    'Battle' => array(
                        'type' => 'INNER',
                        'fields' => array('Battle.id')
                    )
            ))
        );

        // Get the unique ids from the local and remote reports
        $playerIds = array_unique(array_merge(Set::extract('/Player/id', $localBattles), Set::extract('/Player/id', $battleReportView)));

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
            $transport = $controller->Thrift->getTransportFor('Notification');
            $protocol = $controller->Thrift->getProtocol($transport);
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
