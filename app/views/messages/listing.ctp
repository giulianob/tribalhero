<?php

debug($messages);

$paging = $paginator->params('Message');

$data = array();

foreach ($messages as $message) {
    $isRecipient = $playerId == $message['Message']['recipient_player_id'];
    $subjectAllowedLength = min(strlen($message['Message']['subject']), 30);
    $messageAllowedLength = 90 - strlen($message['Message']['subject']);
    
    $data[] = array(
            'name' => $isRecipient ?  $message['Sender']['name'] : $message['Recipient']['name'],
            'isRecipient' => $isRecipient,
            'id' => $message['Message']['id'],
            'subject' => $text->truncate($message['Message']['subject'], $subjectAllowedLength),
            'preview' => $text->truncate($message['Message']['message'], $messageAllowedLength),
            'date' => $time->niceShort($message['Message']['created']),
            'unread' => $isRecipient ? ($message['Message']['recipient_state'] == 0) : ($message['Message']['sender_state'] == 0)
    );
}

$results = array('pages' => $paging['pageCount'], 'page' => $paging['page'], 'messages' => $data);

echo $javascript->object($results);
