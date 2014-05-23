<?php

class BattleTribe extends AppModel {
    var $name = 'BattleTribe';
    var $belongsTo = array(
        'Battle',
        'Tribe'
    );
}