<?php

class MessageBoardThread extends AppModel {

    var $name = 'MessageBoardThread';
    var $belongsTo = array(
        'Player',
        'LastPostPlayer' => array('className' => 'Player', 'foreignKey' => 'last_post_player_id')
    );
    var $hasMany = array(
        'MessageBoardPost' => array('dependentBatchDelete' => true)
    );
    var $hasAndBelongsToMany = array(
        'PlayerLastRead' => array(
            'with' => 'MessageBoardRead',
            'className' => 'Player'
        )
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
    var $limitPerPage = 20;

    public function getThreadHeader($playerId, $threadId) {
        $tribeId = $this->Player->getTribeId($playerId);

        if (!$tribeId)
            return null;

        $thread = $this->find('first', array(
            'link' => array(
                'Player' => array('fields' => array('id', 'name')),
                'MessageBoardRead' => array('conditions' => array('MessageBoardRead.player_id' => $playerId), 'fields' => array('MessageBoardRead.last_read'))
            ),
            'conditions' => array('MessageBoardThread.id' => $threadId, 'MessageBoardThread.deleted' => 0)
                ));

        if (empty($thread) || $thread['MessageBoardThread']['tribe_id'] !== $tribeId)
            return null;

        return $thread;
    }

    public function addThread($playerId, $editThreadId, $subject, $message) {
        $tribeId = $this->Player->getTribeId($playerId);

        if (!$tribeId)
            return array('success' => false);

        $newMessage = array(
            'player_id' => $playerId,
            'subject' => $subject,
            'message' => $message,
            'deleted' => 0,
            'sticky' => 0,
            'last_post_date' => DboSource::expression('UTC_TIMESTAMP()'),
            'last_post_player_id' => $playerId,
            'tribe_id' => $tribeId
        );

        if ($editThreadId > -1) {
            $thread = $this->findById($editThreadId);

            if (empty($thread) || $thread['MessageBoardThread']['player_id'] != $playerId) {
                return array('success' => false);
            }

            $newMessage['id'] = $editThreadId;
        }

        if ($this->save($newMessage)) {
            return array('success' => true, 'id' => $this->id);
        }
        else {
            $errors = $this->invalidFields();
            return array('success' => false, 'error' => reset($errors));
        }
    }

    public function getListing($playerId, $page) {
        $tribeId = $this->Player->getTribeId($playerId);

        if (!$tribeId)
            return null;

        return array(
            'conditions' => array(
                'MessageBoardThread.tribe_id' => $tribeId,
                'MessageBoardThread.deleted' => 0,
            ),
            'link' => array(
                'Player' => array('fields' => array('Player.id', 'Player.name')),
                'LastPostPlayer' => array('class' => 'Player', 'fields' => array('LastPostPlayer.id', 'LastPostPlayer.name'), 'conditions' => array('exactly' => 'LastPostPlayer.id = MessageBoardThread.last_post_player_id')),
                'MessageBoardRead' => array('conditions' => array('MessageBoardThread.last_post_date >' => $this->toSqlTime(strtotime('-2 weeks')), 'MessageBoardRead.player_id' => $playerId), 'fields' => array('MessageBoardRead.last_read'))
            ),
            'page' => $page,
            'limit' => $this->limitPerPage,
            'order' => array('MessageBoardThread.sticky DESC', 'MessageBoardThread.last_post_date DESC'),
            'fields' => array('MessageBoardThread.last_post_date', 'MessageBoardThread.message_board_post_count', 'MessageBoardThread.subject', 'MessageBoardThread.sticky', 'MessageBoardThread.id'),
        );
    }

    public function markDeleted($playerId, $threadId) {
        $tribesman = $this->Player->Tribesman->Tribe->findTribesman($playerId);

        if (empty($tribesman))
            return false;

        $thread = $this->find('first', array(
            'link' => array('Player' => array('fields' => array('id', 'name'))),
            'conditions' => array('MessageBoardThread.id' => $threadId, 'MessageBoardThread.tribe_id' => $tribesman['Tribesman']['tribe_id'], 'MessageBoardThread.deleted' => 0)
                ));

        if (empty($thread))
            return false;

        // Player should be either poster or high rank enough to delete this post
        if ($thread['MessageBoardThread']['player_id'] !== $playerId && !$this->Player->Tribesman->Tribe->hasRight('post_delete', $tribesman))
            return false;

        $this->id = $threadId;
        $this->save(array(
            'deleted' => true
                ), false);

        return true;
    }

    public function getUnreadMessage($playerId, $tribeId) {
        // This query works by bring in the matching records (w/ left outer the original record will NOT be skipped even if no matching record is found)
        // We then only take the original record if the thread has never been read (MessageBoardRead.is is null) or if there has been a newer post since the player read the thread
        $someUnreadThread = $this->find('first', array(
            'link' => array(
                'MessageBoardRead' => array(
                    'type' => 'LEFT OUTER',
                    'conditions' => array(
                        'MessageBoardRead.player_id' => $playerId                        
                    )
                )
            ),
            'conditions' => array(
                'MessageBoardThread.last_post_date >' => $this->toSqlTime(strtotime('-2 weeks')),
                'MessageBoardThread.tribe_id' => $tribeId,                                
                'MessageBoardThread.deleted' => 0,        
                'OR' => array('MessageBoardRead.id' => null, 'MessageBoardThread.last_post_date > MessageBoardRead.last_read')
        )));

        return !empty($someUnreadThread);
    }

}
