<?php

$isRecipient = $playerId == $message['Message']['recipient_player_id'];
$subjectAllowedLength = min(strlen($message['Message']['subject']), 30);
$messageAllowedLength = 75 - strlen($message['Message']['subject']);

$data = array('message' => array(
				'senderId' => $message['Sender']['id'],
				'recipientId' => $message['Recipient']['id'],
                'name' => $isRecipient ?  $message['Sender']['name'] : $message['Recipient']['name'],
                'isRecipient' => $isRecipient,
                'id' => $message['Message']['id'],
                'subject' => $message['Message']['subject'],
                'message' => $message['Message']['message'],
                'date' => $time->niceShort($message['Message']['created'])
        ),
        'refreshOnClose' => $refreshOnClose
);

echo $this->Js->object($data);