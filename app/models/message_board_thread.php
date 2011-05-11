<?php

class MessageBoardThread extends AppModel {

    var $name = 'MessageBoardThread';
    var $belongsTo = array(
        'Player',
        'LastPostPlayer' => array('className' => 'Player', 'foreignKey' => 'last_post_player_id')
    );
    var $hasMany = array(
        'MessageBoardPost'
    );
    var $validate = array(
        'message' => array(
            'maxLength' => array(
                'rule' => array('maxLength', 30000),
                'message' => 'Message is too long. Shorten the message and try again.',
                'required' => true,
                'allowEmpty' => false
            )
        ),
        'subject' => array(
            'maxLength' => array(
                'rule' => array('maxLength', 150),
                'message' => 'Subject is too long. Shorten the subject and try again.',
                'required' => true,
                'allowEmpty' => false
            )
        )
    );
    var $limitPerPage = 40;

    public function getThreadHeader($playerId, $threadId) {
        $tribeId = $this->Player->getTribeId($playerId);

        if (!$tribeId)
            return null;

        $thread = $this->find('first', array(
                    'link' => array('Player' => array('fields' => array('id', 'name'))),
                    'conditions' => array('MessageBoardThread.id' => $threadId, 'MessageBoardThread.deleted' => 0)
                ));

        if (empty($thread) || $thread['MessageBoardThread']['tribe_id'] !== $tribeId)
            return null;

        return $thread;
    }

    public function addThread($playerId, $subject, $message) {
        $tribeId = $this->Player->getTribeId($playerId);

        if (!$tribeId)
            return array('success' => false);

        $newMessage = array(
            'player_id' => $playerId,
            'subject' => $subject,
            'message' => $message,
            'deleted' => 0,
            'sticky' => 0,
            'last_post_date' => DboSource::expression('NOW()'),
            'last_post_player_id' => $playerId,
            'tribe_id' => $tribeId
        );

        if ($this->save($newMessage)) {
            return array('success' => true, 'id' => $this->id);
        } else {
            $errors = $this->invalidFields();
            return array('success' => false, 'error' => reset($errors));
        }
    }

    public function getListing($playerId, $page) {
        $tribeId = $this->Player->getTribeId($playerId);

        if (!$tribeId)
            return null;

        return array(
            'contain' => array(
                'Player' => array('fields' => array('id', 'name')),
                'LastPostPlayer' => array('fields' => array('id', 'name'))
            ),
            'conditions' => array(
                'MessageBoardThread.tribe_id' => $tribeId,
                'MessageBoardThread.deleted' => 0,
            ),
            'page' => $page,
            'limit' => $this->limitPerPage,
            'order' => array('MessageBoardThread.sticky DESC', 'MessageBoardThread.last_post_date DESC'),
            'fields' => array('last_post_date', 'message_board_post_count', 'subject', 'sticky'),
        );
    }

    public function markDeleted($playerId, $threadId) {
        $tribesman = $this->Player->Tribesman->findByPlayerId($playerId);

        if (empty($tribesman))
            return false;

        $thread = $this->find('first', array(
                    'link' => array('Player' => array('fields' => array('id', 'name'))),
                    'conditions' => array('MessageBoardThread.id' => $threadId, 'MessageBoardThread.tribe_id' => $tribesman['Tribesman']['tribe_id'], 'MessageBoardThread.deleted' => 0)
                ));

        if (empty($thread))
            return false;

        // Player should be either poster or high rank enough to delete this post
        if ($thread['MessageBoardThread']['player_id'] !== $playerId && $tribesman['Tribesman']['rank'] > 1)
            return false;

        $this->id = $threadId;
        $this->save(array(
            'deleted' => true
        ));

        return true;
    }

}