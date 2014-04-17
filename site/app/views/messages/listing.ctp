<?php

$paging = $paginator->params('Message');

$data = array();

foreach ($messages as $message) {

    // Remove new lines
    $message['Message']['message'] = str_replace("\r", "  ", $message['Message']['message']);

    $isRecipient = $playerId == $message['Message']['recipient_player_id'];    
    $messageAllowedLength = 75 - strlen($message['Message']['subject']);   

	if ($message['Sender']['id'] == 0) {
		$message['Sender']['name'] = 'System';
	}
	
    $data[] = array(
            'name' => $isRecipient ?  $message['Sender']['name'] : $message['Recipient']['name'],
            'isRecipient' => $isRecipient,
            'id' => $message['Message']['id'],
            'subject' => $message['Message']['subject'],
            'preview' => $text->truncate($message['Message']['message'], $messageAllowedLength),
            'date' => strtotime($message['Message']['created']),
            'unread' => $isRecipient ? ($message['Message']['recipient_state'] == 0) : false
    );
}

$results = array('pages' => $paging['pageCount'], 'page' => $paging['page'], 'messages' => $data);

echo $this->Js->object($results);
