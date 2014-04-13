<?php

class Stronghold extends AppModel {

    var $name = 'Stronghold';
    var $belongsTo = array(
        'Tribe',
    );

}