<?php
/* Tribesman Fixture generated on: 2011-05-08 18:05:54 : 1304880294 */
class TribesmanFixture extends CakeTestFixture {
	var $name = 'Tribesman';

	var $fields = array(
		'player_id' => array('type' => 'integer', 'null' => false, 'default' => '0', 'key' => 'primary'),
		'tribe_id' => array('type' => 'integer', 'null' => false, 'default' => NULL),
		'join_date' => array('type' => 'datetime', 'null' => false, 'default' => NULL),
		'rank' => array('type' => 'integer', 'null' => true, 'default' => NULL, 'length' => 3),
		'crop' => array('type' => 'integer', 'null' => false, 'default' => NULL),
		'gold' => array('type' => 'integer', 'null' => false, 'default' => NULL),
		'iron' => array('type' => 'integer', 'null' => false, 'default' => NULL),
		'wood' => array('type' => 'integer', 'null' => false, 'default' => NULL),
		'indexes' => array('PRIMARY' => array('column' => 'player_id', 'unique' => 1)),
		'tableParameters' => array('charset' => 'latin1', 'collate' => 'latin1_swedish_ci', 'engine' => 'InnoDB')
	);

	var $records = array(
		array(
			'player_id' => 1,
			'tribe_id' => 1,
			'join_date' => '2011-05-08 18:44:54',
			'rank' => 1,
			'crop' => 1,
			'gold' => 1,
			'iron' => 1,
			'wood' => 1
		),
	);
}
?>