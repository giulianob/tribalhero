<?php

/**
 * Clears the old battle reports and messages
 * @property Battle $Battle
 * @property Message $Message
 * @property MessageBoardRead $MessageBoardRead
 * @property MessageBoardThread $MessageBoardThread
 */
class ClearDbShell extends Shell {

    var $uses = array('Battle', 'Message', 'MessageBoardRead', 'MessageBoardThread');

    function main() {
        gc_enable();

        // Delete reports 1 by 1
        $battles = $this->Battle->find('all', array(
            'conditions' => array(
                array('NOT' => array(
                        'Battle.ended' => null
                )),
                'Battle.ended < ' => date("Y-m-d H:i:s", strtotime('-2 weeks'))
            ),
            'fields' => array('id'),
            'contain' => array()
                ));

        $this->out(sprintf("Deleting %d battles", count($battles)));

        $deleted = 0;
        $battleCount = count($battles);
        foreach ($battles as $battle) {

            $reports = $this->Battle->BattleReport->findAllByBattleId($battle['Battle']['id']);

            foreach ($reports as $report) {
                if (!$this->Battle->BattleReport->delete($report['BattleReport']['id'])) {
                    $this->out("Failed to delete battle report %d", $report['BattleReport']['id']);
                    exit(1);
                }
            }

            unset($reports);

            if (!$this->Battle->delete($battle['Battle']['id'])) {
                $this->out("Failed to delete battle %d", $battle['Battle']['id']);
                exit(1);
            }

            $deleted++;
            if ($deleted % 50 == 0) {
                $this->out(sprintf("%d/%d", $deleted, $battleCount));
                gc_collect_cycles();
            }
        }

        // Delete messages
        $this->out("Deleting old messages...");
        $this->Message->deleteAll(array('Message.created <' => date("Y-m-d H:i:s", strtotime('-4 weeks'))), false, false);
        $this->Message->deleteAll(array('Message.sender_player_id' => 0, 'Message.created <' => date("Y-m-d H:i:s", strtotime('-2 weeks'))), false, false);
        $this->MessageBoardRead->deleteAll(array('MessageBoardRead.last_read <' => date("Y-m-d H:i:s", strtotime('-4 weeks'))), false, false);

        $this->out("Deleting old forum messages...");
        $this->MessageBoardThread->unbindModel(array('hasAndBelongsToMany' => array('Player')), false);
        $posts = $this->MessageBoardThread->find('all', array(
            'conditions' => array(
                'MessageBoardThread.last_post_date < ' => date("Y-m-d H:i:s", strtotime('-4 weeks'))
            ),
            'fields' => array('id'),
            'contain' => array()
        ));
        foreach ($posts as $post) {
            $this->MessageBoardThread->delete($post['MessageBoardThread']['id']);
        }
        $this->MessageBoardThread->resetAssociations();

        $this->out("Done");
    }

}