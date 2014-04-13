<?php
/* MessageBoardPost Test cases generated on: 2011-05-08 18:05:31 : 1304879491*/
App::import('Model', 'MessageBoardPost');

class MessageBoardPostTestCase extends CakeTestCase {
	var $fixtures = array('app.message_board_post', 'app.message_board_thread');

	function startTest() {
		$this->MessageBoardPost =& ClassRegistry::init('MessageBoardPost');
	}

	function endTest() {
		unset($this->MessageBoardPost);
		ClassRegistry::flush();
	}

}
?>