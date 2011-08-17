<?php

class Tribe extends AppModel {

    var $name = 'Tribe';
    var $primaryKey = 'player_id';
    var $belongsTo = array(
        'Player',
    );
    var $hasMany = array(
        'Tribesman',
    );

}