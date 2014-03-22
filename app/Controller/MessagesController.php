<?php

class MessagesController extends AppController {

    var $helpers = array('Time', 'Text');
    var $allowedFromGame = array('listing', 'view', 'del', 'mark_as_read', 'send');

    function del() {
        $playerId = $this->request->data['playerId'];
        $messageIds = array_key_exists('ids', $this->request->data) ? $this->request->data['ids'] : array();
        if (!is_array($messageIds)) {
            $messageIds = array($messageIds);
        }

        $this->Message->sendToTrash($playerId, $messageIds);

        $data = array('success' => true);

        $this->set('data', $data);
        $this->render('/elements/to_json');
    }

    function mark_as_read() {
        $playerId = $this->request->data['playerId'];
        $messageIds = array_key_exists('ids', $this->request->data) ? $this->request->data['ids'] : array();
        if (!is_array($messageIds)) {
            $messageIds = array($messageIds);
        }

        $this->Message->markAsRead($playerId, $messageIds);

        $data = array('success' => true);

        $this->set('data', $data);
        $this->render('/elements/to_json');
    }

    function listing() {
        $playerId = $this->request->data['playerId'];
        $page = array_key_exists('page', $this->request->data) ? intval($this->request->data['page']) : 0;
        $folder = array_key_exists('folder', $this->request->data) ? $this->request->data['folder'] : "inbox";

        switch ($folder) {
            case "sent":
                $this->paginate = $this->Message->getSent($playerId, $page);
                break;
            default: // Inbox
                $this->paginate = $this->Message->getInbox($playerId, $page);
                break;
        }

        // The view uses the playerId to determine whether the message is from/to this player
        $this->set('playerId', $playerId);
        $this->set('messages', $this->paginate('Message'));
    }

    function view($id = null) {
        $playerId = $this->request->data['playerId'];
        $id = array_key_exists('id', $this->request->data) ? intval($this->request->data['id']) : -1;

        $message = $this->Message->getMessageForPlayer($playerId, $id);

        if (empty($message)) {
            $data = array('error' => 'The specified message could not be found');
            $this->set(compact('data'));
            $this->render('/elements/to_json');
            return;
        }

        // Set as read if necessary
        $refreshOnClose = false;
        if ($playerId == $message['Message']['recipient_player_id'] && $message['Message']['recipient_state'] == $this->Message->states['unread']) {
            $this->Message->markAsRead($playerId, $message['Message']['id']);
            $refreshOnClose = true;
        }

        $this->set('refreshOnClose', $refreshOnClose);
        $this->set('playerId', $playerId);
        $this->set(compact('message'));
    }

    function send() {
        $playerId = $this->request->data['playerId'];
        $subject = $this->request->data['subject'];
        $message = $this->request->data['message'];
        $to = $this->request->data['to'];

        // Only allow 1 message every x seconds
        $lastMessage = $this->Message->find('first', array(
                    'contain' => array(),
                    'fields' => array('Message.created'),
                    'conditions' => array(
                        'Message.sender_player_id' => $playerId
                    ),
                    'order' => 'Message.created DESC'
                ));

        if (!empty($lastMessage) && time() - strtotime($lastMessage['Message']['created']) < 10) {
            $data = array('error' => 'You are sending messages too fast. Please wait a few more seconds and try again.');
        } else {
            $Recipient =& ClassRegistry::init('Recipient');
            
            // Find recipient
            $recipient = $Recipient->findByName($to);

            if (empty($recipient)) {
                $data = array('error' => 'Recipient not found.');
            }
            else
            {
                $data = $this->Message->send($playerId, $recipient, $subject, $message);                               
                
                // If successful, then send RPC w/ new message count
                if (get_value($data, 'success'))
                {
                    $unreadMessages = $this->Message->getUnreadCount($recipient['Recipient']['id']);
                    
                    try
                    {
                        $transport = $this->Thrift->getTransportFor('Notification');
                        $protocol = $this->Thrift->getProtocol($transport);
                        $notificationRpc = new NotificationClient($protocol);
                        $transport->open();                                                     
                        $notificationRpc->NewMessage(new PlayerUnreadCount(array('id' => $recipient['Recipient']['id'], 'unreadCount' => $unreadMessages)));
                        $transport->close();
                    }
                    catch (Exception $e)
                    {
                        debug($e);
                    }
                }                
            }
        }
        
        $this->set(compact('data'));
        $this->render('/elements/to_json');
    }

}