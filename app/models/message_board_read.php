<?php

class MessageBoardRead extends AppModel {
    
    var $useTable = 'message_board_read';
    
    var $belongsTo = array(
        'Player',
        'MessageBoardThread'
    );

}