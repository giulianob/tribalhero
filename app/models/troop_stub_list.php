<?php
class TroopStubList extends AppModel {
	
	var $name = 'TroopStubList';
	var $useTable = 'troop_stubs_list';
	var $belongsTo = array(
		'City' => array(
			'className' => 'City',
			'foreignKey' => 'city_id',
			'conditions' => '',
			'fields' => '',
			'order' => ''
		)
	);
	
	
}