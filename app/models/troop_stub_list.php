<?php

class TroopStubList extends AppModel {

    var $name = 'TroopStubList';
    var $useTable = 'troop_stubs_list';
    var $belongsTo = array(
        'City',
    );

}