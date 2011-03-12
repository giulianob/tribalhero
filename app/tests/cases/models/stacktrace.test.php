<?php
/* Stacktrace Test cases generated on: 2011-03-11 04:03:59 : 1299818819*/
App::import('Model', 'Stacktrace');

class StacktraceTestCase extends CakeTestCase {
	var $fixtures = array('app.stacktrace', 'app.player', 'app.city', 'app.battle_report_troop', 'app.battle_report', 'app.battle', 'app.battle_report_view', 'app.battle_report_object', 'app.troop_stub_list', 'app.message');

	function startTest() {
		$this->Stacktrace =& ClassRegistry::init('Stacktrace');
	}

	function endTest() {
		unset($this->Stacktrace);
		ClassRegistry::flush();
	}

}
?>