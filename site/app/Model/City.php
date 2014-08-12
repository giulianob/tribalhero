<?php

class City extends AppModel {

    var $belongsTo = array(
        'Player'
    );

    var $hasMany = array(
        'TroopStubList'
    );

}