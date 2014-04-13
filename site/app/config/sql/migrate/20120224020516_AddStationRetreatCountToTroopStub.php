<?php

class AddStationRetreatCountToTroopStub extends Ruckusing_BaseMigration {

	public function up() {
		$this->add_column("troop_stubs", "retreat_count", 'smallinteger', array('length' => 5, 'unsigned' => true, 'null' => false) );
	}//up()

	public function down() {
		$this->remove_column("troop_stubs","retreat_count");
	}//down()
}
?>