<?php

class AddGateOpenToToStronghold extends Ruckusing_BaseMigration {

	public function strtoupper() {
		 $this->add_column('strongholds', 'gate_open_to', 'integer', array('null' => false, 'unsigned' => true));
	}//strtoupper()

	public function down() {
		$this->remove_column('strongholds', 'gate_open_to');	
	}//down()
}
?>