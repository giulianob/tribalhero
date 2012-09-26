<?php

class Tribesman extends AppModel {

    var $name = 'Tribesman';
    var $useTable = 'tribesmen';
    var $primaryKey = 'player_id';
 
    var $belongsTo = array(
        'Player',
        'Tribe'
    );
}