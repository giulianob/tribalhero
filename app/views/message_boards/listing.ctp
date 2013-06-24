<?php

$paging = $paginator->params('MessageBoardThread');

$data = array();

// Posts older than two weeks should just get a current timestamp
$twoWeeksAgo = strtotime('-2 weeks');
$now = time();

foreach ($messages as $message) {

    $lastRead = strtotime($message['MessageBoardRead']['last_read']);
    $lastPostDate = strtotime($message['MessageBoardThread']['last_post_date']);

    if (!$lastRead && $lastPostDate < $twoWeeksAgo) {
        $lastRead = time();
    }

    $data[] = array(
        'id' => $message['MessageBoardThread']['id'],
        'lastPostTimestamp' => $lastPostDate,
        'postCount' => $message['MessageBoardThread']['message_board_post_count'],
        'subject' => $message['MessageBoardThread']['subject'],
        'playerId' => $message['Player']['id'],
        'playerName' => $message['Player']['name'],
        'lastPostPlayerId' => $message['LastPostPlayer']['id'],
        'lastPostPlayerName' => $message['LastPostPlayer']['name'],
        'sticky' => $message['MessageBoardThread']['sticky'],
        'lastReadTimestamp' => $lastRead,
    );
}

$results = array('pages' => $paging['pageCount'], 'page' => $paging['page'], 'messages' => $data);

echo $this->Js->object($results);
