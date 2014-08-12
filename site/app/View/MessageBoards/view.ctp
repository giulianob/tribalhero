<?php

$paging = $this->Paginator->params('MessageBoardPost');

$result = array(
    'pages' => $paging['pageCount'],
    'page' => $paging['page'],
    'thread' => array(
        'id' => $thread['MessageBoardThread']['id'],
        'createdTimestamp' => strtotime($thread['MessageBoardThread']['created']),
        'subject' => $thread['MessageBoardThread']['subject'],
        'message' => $thread['MessageBoardThread']['message'],
        'playerName' => $thread['Player']['name'],
	'playerId' => $thread['Player']['id'],
        'lastReadTimestamp' => strtotime($thread['MessageBoardRead']['last_read']),
    ),
    'posts' => array()
);

foreach ($posts as $post) {
    $result['posts'][] = array(
        'id' => $post['MessageBoardPost']['id'],
        'createdTimestamp' => strtotime($post['MessageBoardPost']['created']),
        'message' => $post['MessageBoardPost']['message'],
        'playerId' => $post['Player']['id'],
        'playerName' => $post['Player']['name'],
    );
}

echo $this->Js->object($result);