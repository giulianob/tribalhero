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
        foreach ($battles as $battle) {
            if (!$this->Battle->delete($battle['Battle']['id'])) {
                $this->out("Failed to delete battle %d", $battle['Battle']['id']);
                exit(1);
            }
            $deleted++;
            if ($deleted % 50 == 0) {
                $this->out(sprintf("%d/%d", $deleted, count($battles)));
            }
        }

        // Delete messages
        $this->out("Deleting old messages...");
        $this->Message->deleteAll(array('Message.created <' => date("Y-m-d H:i:s", strtotime('-4 weeks'))), false, false);
        $this->Message->deleteAll(array('Message.sender_player_id' => 0, 'Message.created <' => date("Y-m-d H:i:s", strtotime('-2 weeks'))), false, false);
        $this->MessageBoardRead->deleteAll(array('MessageBoardRead.last_read <' => date("Y-m-d H:i:s", strtotime('-2 weeks'))), false, false);

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