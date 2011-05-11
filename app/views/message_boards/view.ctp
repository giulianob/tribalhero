<?php

$paging = $paginator->params('MessageBoardPost');

$result = array(
    'pages' => $paging['pageCount'],
    'page' => $paging['page'],
    'thread' => array(
        'id' => $thread['MessageBoardThread']['id'],
        'created' => $time->niceShort($thread['MessageBoardThread']['created']),
        'subject' => $thread['MessageBoardThread']['subject'],
        'message' => $thread['MessageBoardThread']['message'],
        'player_name' => $thread['Player']['name'],
    ),
    'posts' => array()
);

foreach ($posts as $post) {
    $result['posts'][] = array(
        'id' => $post['MessageBoardPost']['id'],
        'created' => $time->niceShort($post['MessageBoardPost']['created']),
        'message' => $post['MessageBoardPost']['message'],
        'player_id' => $post['Player']['id'],
        'player_name' => $post['Player']['name'],
    );
}

echo $this->Js->object($result);