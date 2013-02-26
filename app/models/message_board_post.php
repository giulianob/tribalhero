<?php

class MessageBoardPost extends AppModel {

    var $name = 'MessageBoardPost';
    var $belongsTo = array(
        'MessageBoardThread' => array(
            'counterCache' => true,
            'counterScope' => array('MessageBoardThread.deleted' => 0)
        ),
        'Player'
    );
    var $validate = array(
        'message' => array(
            'maxLength' => array(
                'rule' => array('maxLength', 30000),
                'message' => 'Message is too long. Shorten the message and try again.',
                'required' => true,
                'allowEmpty' => false
            )
        )
    );
    var $limitPerPage = 20;

    public function getPostsForThread($threadId, $page) {
        return array(
            'contain' => array(
                'Player' => array('fields' => array('id', 'name'))
            ),
            'conditions' => array(
                'MessageBoardPost.message_board_thread_id' => $threadId,
                'MessageBoardPost.deleted' => 0,
            ),
            'page' => $page,
            'limit' => $this->limitPerPage,
            'order' => array('MessageBoardPost.created ASC'),
        );
    }

    public function addPost($playerId, $threadId, $editPostId, $message) {

        $tribesman = $this->Player->Tribesman->findByPlayerId($playerId);

        if (empty($tribesman))
            return array('success' => false);

        $newMessage = array(
            'player_id' => $playerId,
            'message_board_thread_id' => $threadId,
            'message' => $message,
            'deleted' => 0,
        );

        $thread = $this->MessageBoardThread->find('first', array(
            'conditions' => array('MessageBoardThread.id' => $threadId, 'MessageBoardThread.tribe_id' => $tribesman['Tribesman']['tribe_id'])
        ));

        if (empty($thread)) {
            return array('success' => false);
        }

        if ($editPostId > -1) {
            $newMessage['id'] = $editPostId;

            $post = $this->find('first', array(
                'conditions' => array('id' => $editPostId, 'message_board_thread_id' => $thread['MessageBoardThread']['id'])
            ));

            if (empty($post) || $post['MessageBoardPost']['player_id'] != $playerId) {
                return array('success' => false);
            }
        }

        if ($this->save($newMessage)) {

            $this->MessageBoardThread->create();
            $this->MessageBoardThread->id = $threadId;
            $this->MessageBoardThread->save(array(
                'last_post_date' => DboSource::expression('UTC_TIMESTAMP()'),
                'last_post_player_id' => $playerId,
                    ), false);

            $this->MessageBoardThread->create();
            
            $count = $this->MessageBoardThread->find('count', array(
                        'conditions' => array('MessageBoardThread.id' => $threadId)
                    ));
            return array('success' => true, 'id' => $this->id, 'pages' => ceil($count / $this->limitPerPage));
        } else {
            $errors = $this->invalidFields();
            return array('success' => false, 'error' => reset($errors));
        }
    }

    public function markDeleted($playerId, $postId) {
        $tribesman = $this->Player->Tribesman->findByPlayerId($playerId);

        if (empty($tribesman))
            return false;

        $post = $this->find('first', array(
                    'link' => array(
                        'Player' => array('fields' => array('id', 'name')),
                        'MessageBoardThread' => array(
                            'type' => 'INNER',
                            'conditions' => array('MessageBoardThread.tribe_id' => $tribesman['Tribesman']['tribe_id']),
                        ),
                    ),
                    'conditions' => array('MessageBoardPost.id' => $postId, 'MessageBoardPost.deleted' => 0)
                ));

        if (empty($post))
            return false;

        // Player should be either poster or high rank enough to delete this post
        if ($post['MessageBoardPost']['player_id'] !== $playerId && $tribesman['Tribesman']['rank'] > 1)
            return false;
        
        $this->id = $postId;
        $this->save(array(
            'deleted' => true
        ), false);

        return true;
    }

}