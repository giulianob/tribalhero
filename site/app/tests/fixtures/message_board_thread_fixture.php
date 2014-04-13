<?php
/* MessageBoardThread Fixture generated on: 2011-05-08 18:05:26 : 1304879546 */
class MessageBoardThreadFixture extends CakeTestFixture {
	var $name = 'MessageBoardThread';

	var $fields = array(
		'id' => array('type' => 'integer', 'null' => false, 'default' => NULL, 'key' => 'primary'),
		'tribe_id' => array('type' => 'integer', 'null' => false, 'default' => NULL, 'key' => 'index'),
		'created' => array('type' => 'datetime', 'null' => false, 'default' => NULL),
		'player_id' => array('type' => 'integer', 'null' => false, 'default' => NULL),
		'last_post_date' => array('type' => 'datetime', 'null' => false, 'default' => NULL),
		'last_post_player_id' => array('type' => 'integer', 'null' => false, 'default' => NULL),
		'message_board_post_count' => array('type' => 'integer', 'null' => false, 'default' => NULL),
		'subject' => array('type' => 'string', 'null' => false, 'default' => NULL, 'collate' => 'latin1_swedish_ci', 'charset' => 'latin1'),
		'message' => array('type' => 'text', 'null' => false, 'default' => NULL, 'collate' => 'latin1_swedish_ci', 'charset' => 'latin1'),
		'deleted' => array('type' => 'boolean', 'null' => false, 'default' => NULL),
		'sticky' => array('type' => 'boolean', 'null' => false, 'default' => NULL, 'key' => 'index'),
		'indexes' => array('PRIMARY' => array('column' => 'id', 'unique' => 1), 'idx_message_board_threads_tribe_id' => array('column' => 'tribe_id', 'unique' => 0), 'idx_message_board_threads_sticky_and_tribe_id_and_last_post_date' => array('column' => array('sticky', 'tribe_id', 'last_post_date'), 'unique' => 0)),
		'tableParameters' => array('charset' => 'latin1', 'collate' => 'latin1_swedish_ci', 'engine' => 'InnoDB')
	);

	var $records = array(
		array(
			'id' => 1,
			'tribe_id' => 1,
			'created' => '2011-05-08 18:32:26',
			'player_id' => 1,
			'last_post_date' => '2011-05-08 18:32:26',
			'last_post_player_id' => 1,
			'message_board_post_count' => 1,
			'subject' => 'Lorem ipsum dolor sit amet',
			'message' => 'Lorem ipsum dolor sit amet, aliquet feugiat. Convallis morbi fringilla gravida, phasellus feugiat dapibus velit nunc, pulvinar eget sollicitudin venenatis cum nullam, vivamus ut a sed, mollitia lectus. Nulla vestibulum massa neque ut et, id hendrerit sit, feugiat in taciti enim proin nibh, tempor dignissim, rhoncus duis vestibulum nunc mattis convallis.',
			'deleted' => 1,
			'sticky' => 1
		),
	);
}
?>