<?php

/**
 * @property MessageBoardThread $MessageBoardThread
 * @property MessageBoardPost $MessageBoardPost
 */
class MessageBoardsController extends AppController {

    var $helpers = array('Time', 'Text');
    var $uses = array('MessageBoardThread', 'MessageBoardPost');
    var $allowedFromGame = array('listing', 'view', 'del_thread', 'del_post', 'add_thread', 'add_post', 'sticky_thread');

    function listing() {
        $playerId = $this->params['form']['playerId'];
        $page = array_key_exists('page', $this->params['form']) ? intval($this->params['form']['page']) : 0;

        $this->paginate = $this->MessageBoardThread->getListing($playerId, $page);

        if (empty($this->paginate)) {
            $data = array('success' => false);
            $this->set('data', $data);
            $this->render('/elements/to_json');
            return;
        }

        $messages = $this->paginate('MessageBoardThread');       
        
        $this->set('messages', $messages);
    }

    function view() {
        $playerId = $this->params['form']['playerId'];
        $page = array_key_exists('page', $this->params['form']) ? intval($this->params['form']['page']) : 0;
        $id = array_key_exists('id', $this->params['form']) ? intval($this->params['form']['id']) : -1;

        $thread = $this->MessageBoardThread->getThreadHeader($playerId, $id);

        if (empty($thread)) {
            $data = array('success' => false, 'error' => 'Thread specified does not exist');
            $this->set('data', $data);
            $this->render('/elements/to_json');
            return;
        }

        $this->paginate = $this->MessageBoardPost->getPostsForThread($id, $page);

        $this->MessageBoardThread->MessageBoardRead->updateLastRead($playerId, $id);
        
        $this->set('thread', $thread);
        $this->set('posts', $this->paginate('MessageBoardPost'));
    }

    function del_thread() {
        $playerId = $this->params['form']['playerId'];
        $id = array_key_exists('id', $this->params['form']) ? intval($this->params['form']['id']) : -1;

        if (!$this->MessageBoardThread->markDeleted($playerId, $id))
            $data = array('success' => false, 'error' => 'An error occurred while deleting your post');        
        else
            $data = array('success' => true);
        
        $data = array('success' => true);
        $this->set('data', $data);
        $this->render('/elements/to_json');
    }

    function del_post() {
        $playerId = $this->params['form']['playerId'];
        $id = array_key_exists('id', $this->params['form']) ? intval($this->params['form']['id']) : -1;

        if (!$this->MessageBoardPost->markDeleted($playerId, $id))
            $data = array('success' => false, 'error' => 'An error occurred while deleting your post');        
        else
            $data = array('success' => true);
        
        $this->MessageBoardThread->MessageBoardRead->updateLastRead($playerId, $id);
        
        $this->set('data', $data);
        $this->render('/elements/to_json');
    }

    function add_thread() {
        $playerId = $this->params['form']['playerId'];
        $editThreadId = $this->params['form']['editThreadId'];
        $subject = trim($this->params['form']['subject']);
        $message = rtrim($this->params['form']['message']);

        $data = $this->MessageBoardThread->addThread($playerId, $editThreadId, $subject, $message);
        $this->MessageBoardThread->MessageBoardRead->updateLastRead($playerId, $data['id']);
 
        $this->_signalServerWithNewTribeForumPost($playerId);

        $this->set(compact('data'));
        $this->render('/elements/to_json');
    }

    function add_post() {
        $playerId = $this->params['form']['playerId'];
        $threadId = $this->params['form']['threadId'];
        $editPostId = $this->params['form']['editPostId'];
        $message = rtrim($this->params['form']['message']);

        $thread = $this->MessageBoardThread->getThreadHeader($playerId, $threadId);

        if (empty($thread)) {
            $data = array('success' => false, 'error' => 'Thread specified does not exist');
        } else {
            $data = $this->MessageBoardPost->addPost($playerId, $threadId, $editPostId, $message);
            $this->MessageBoardThread->MessageBoardRead->updateLastRead($playerId, $threadId);
            $this->_signalServerWithNewTribeForumPost($playerId);
        }
        
        $this->set(compact('data'));
        $this->render('/elements/to_json');
    }

    function sticky_thread() {
        
    }

    private function _signalServerWithNewTribeForumPost($playerId) {
        try
        {
            $transport = $this->Thrift->getTransportFor('Notification');
            $protocol = $this->Thrift->getProtocol($transport);
            $notificationRpc = new NotificationClient($protocol);
            $transport->open();                                                     
            $notificationRpc->NewTribeForumPost($this->MessageBoardThread->Player->getTribeId($playerId), $playerId);
            $transport->close();
        }
        catch (Exception $e)
        {
            debug($e);
        }
    }
}
