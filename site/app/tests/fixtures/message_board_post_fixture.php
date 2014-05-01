<?php
/* MessageBoardPost Fixture generated on: 2011-05-08 18:05:31 : 1304879491 */
class MessageBoardPostFixture extends CakeTestFixture {
	var $name = 'MessageBoardPost';

	var $fields = array(
		'id' => array('type' => 'integer', 'null' => false, 'default' => NULL, 'key' => 'primary'),
		'message_board_thread_id' => array('type' => 'integer', 'null' => false, 'default' => NULL, 'key' => 'index'),
		'player_id' => array('type' => 'integer', 'null' => false, 'default' => NULL),
		'created' => array('type' => 'datetime', 'null' => false, 'default' => NULL),
		'message' => array('type' => 'text', 'null' => false, 'default' => NULL, 'collate' => 'latin1_swedish_ci', 'charset' => 'latin1'),
		'deleted' => array('type' => 'boolean', 'null' => false, 'default' => NULL),
		'indexes' => array('PRIMARY' => array('column' => 'id', 'unique' => 1), 'idx_message_board_posts_message_board_thread_id_and_created' => array('column' => array('message_board_thread_id', 'created'), 'unique' => 0)),
		'tableParameters' => array('charset' => 'latin1', 'collate' => 'latin1_swedish_ci', 'engine' => 'InnoDB')
	);

	var $records = array(
		array(
			'id' => 1,
			'message_board_thread_id' => 1,
			'player_id' => 1,
			'created' => '2011-05-08 18:31:31',
			'message' => 'Lorem ipsum dolor sit amet, aliquet feugiat. Convallis morbi fringilla gravida, phasellus feugiat dapibus velit nunc, pulvinar eget sollicitudin venenatis cum nullam, vivamus ut a sed, mollitia lectus. Nulla vestibulum massa neque ut et, id hendrerit sit, feugiat in taciti enim proin nibh, tempor dignissim, rhoncus duis vestibulum nunc mattis convallis.',
			'deleted' => 1
		),
	);
}
?>