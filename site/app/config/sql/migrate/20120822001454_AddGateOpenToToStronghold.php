<?php

class AddGateOpenToToStronghold extends Ruckusing_BaseMigration {

	public function up() {
		 $this->add_column('strongholds', 'gate_open_to', 'integer', array('null' => false, 'unsigned' => true));
	}//up()

	public function down() {
		$this->remove_column('strongholds', 'gate_open_to');	
	}//down()
}
?>