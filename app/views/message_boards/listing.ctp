<?php

$paging = $paginator->params('MessageBoardThread');

$data = array();

foreach ($messages as $message) {

    $data[] = array(
        'id' => $message['MessageBoardThread']['id'],
        'lastPostAgoInWords' => $time->timeAgoInWords($message['MessageBoardThread']['last_post_date']),
        'lastPostDate' => $time->niceShort($message['MessageBoardThread']['last_post_date']),
        'postCount' => $message['MessageBoardThread']['message_board_post_count'],        
        'subject' => $message['MessageBoardThread']['subject'],
        'playerId' => $message['Player']['id'],
        'playerName' => $message['Player']['name'],
        'lastPostPlayerId' => $message['LastPostPlayer']['id'],
        'lastPostPlayerName' => $message['LastPostPlayer']['name'],
        'sticky' => $message['MessageBoardThread']['sticky'],        
    );
}

$results = array('pages' => $paging['pageCount'], 'page' => $paging['page'], 'messages' => $data);

echo $this->Js->object($results);
