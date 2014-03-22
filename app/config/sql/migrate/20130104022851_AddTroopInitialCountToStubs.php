<?php

class AddTroopInitialCountToStubs extends Ruckusing_BaseMigration {

	public function strtoupper() {
		$this->add_column("troop_stubs", "initial_count", 'smallinteger', array('length' => 5, 'unsigned' => true, 'null' => false, 'default' =>0) );
		$this->add_column("troop_stubs", "attack_mode", 'boolean', array('null' => false, 'limit' => 3, 'unsigned' => true) );
	}//strtoupper()

	public function down() {
		$this->remove_column("troop_stubs", "initial_count");
		$this->remove_column("troop_stubs", "attack_mode");
	}//down()
}
?>