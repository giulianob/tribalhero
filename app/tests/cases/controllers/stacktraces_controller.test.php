<?php
/* Stacktraces Test cases generated on: 2011-03-11 04:03:20 : 1299818840*/
App::import('Controller', 'Stacktraces');

class TestStacktracesController extends StacktracesController {
	var $autoRender = false;

	function redirect($url, $status = null, $exit = true) {
		$this->redirectUrl = $url;
	}
}

class StacktracesControllerTestCase extends CakeTestCase {
	var $fixtures = array('app.stacktrace', 'app.player', 'app.city', 'app.battle_report_troop', 'app.battle_report', 'app.battle', 'app.battle_report_view', 'app.battle_report_object', 'app.troop_stub_list', 'app.message');

	function startTest() {
		$this->Stacktraces =& new TestStacktracesController();
		$this->Stacktraces->constructClasses();
	}

	function endTest() {
		unset($this->Stacktraces);
		ClassRegistry::flush();
	}

}
?>