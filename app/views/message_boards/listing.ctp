<?php

$paging = $paginator->params('MessageBoardThread');

$data = array();

foreach ($messages as $message) {

    $data[] = array(
        'last_post_date' => $time->niceShort($message['MessageBoardThread']['last_post_date']),
        'post_count' => $message['MessageBoardThread']['message_board_post_count'],
        'subject' => $message['MessageBoardThread']['subject'],
        'player_id' => $message['Player']['id'],
        'player_name' => $message['Player']['name'],
        'last_post_player_id' => $message['LastPostPlayer']['id'],
        'last_post_player_id' => $message['LastPostPlayer']['name'],
        'sticky' => $message['MessageBoardThread']['sticky'],
    );
}

$results = array('pages' => $paging['pageCount'], 'page' => $paging['page'], 'messages' => $data);

echo $this->Js->object($results);
