<?php
/* Tribe Test cases generated on: 2011-05-08 18:05:16 : 1304880256*/
App::import('Model', 'Tribe');

class TribeTestCase extends CakeTestCase {
	var $fixtures = array('app.tribe', 'app.tribesman');

	function startTest() {
		$this->Tribe =& ClassRegistry::init('Tribe');
	}

	function endTest() {
		unset($this->Tribe);
		ClassRegistry::flush();
	}

}
?>