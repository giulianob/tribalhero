<?php

class AddDefensiveAssignment extends Ruckusing_BaseMigration {

	public function strtoupper() {
		$this->add_column("assignments", "is_attack", "boolean", array('null' => false, 'unsigned' => true, 'default' => 1));
	}//strtoupper()

	public function down() {
		$this->remove_column("assignments", "is_attack");
	}//down()
}
?>