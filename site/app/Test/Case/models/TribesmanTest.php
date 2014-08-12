<?php
/* Tribesman Test cases generated on: 2011-05-08 18:05:54 : 1304880294*/
App::import('Model', 'Tribesman');

class TribesmanTest extends CakeTestCase {
	var $fixtures = array('app.tribesman', 'app.player', 'app.city', 'app.battle_report_troop', 'app.battle_report', 'app.battle', 'app.battle_report_view', 'app.battle_report_object', 'app.troop_stub_list', 'app.message', 'app.tribe');

	function startTest() {
		$this->Tribesman =& ClassRegistry::init('Tribesman');
	}

	function endTest() {
		unset($this->Tribesman);
		ClassRegistry::flush();
	}

}
?>