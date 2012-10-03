<?php

class Tribe extends AppModel {

    var $name = 'Tribe';

    var $belongsTo = array(
        'Player',
    );
    var $hasMany = array(
        'Tribesman',
    );

}