<?php

class MessageBoardRead extends AppModel {

    var $useTable = 'message_board_read';
    var $belongsTo = array(
        'Player',
        'MessageBoardThread'
    );

    function updateLastRead($playerId, $messageBoardThreadId) {
        $messageBoardRead = $this->findByPlayerIdAndMessageBoardThreadId($playerId, $messageBoardThreadId);
        
        $data = array('last_read' => DboSource::expression('UTC_TIMESTAMP()'));
               
        if ($messageBoardRead) {
            $this->id = $messageBoardRead['MessageBoardRead']['id'];
        }
        else {
            $data = array_merge(array('player_id' => $playerId, 'message_board_thread_id' => $messageBoardThreadId), $data);
        }
        
        $this->save($data);
    }

}
